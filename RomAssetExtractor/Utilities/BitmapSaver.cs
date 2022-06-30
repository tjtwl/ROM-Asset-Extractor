using System;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace RomAssetExtractor.Utilities
{
    public class BitmapSaver
    {
        private readonly ConcurrentQueue<SaveableBitmap> bitmaps = new ConcurrentQueue<SaveableBitmap>();
        private bool isReadyToStart = true;
        private bool isWritingNewBitmaps = false;
        private object lockObject = new object();

        public void Add(SaveableBitmap saveableBitmap)
        {
            lock(lockObject)
                bitmaps.Enqueue(saveableBitmap);
        }

        public void Process()
        {
            lock (lockObject)
            {
                if (!bitmaps.TryDequeue(out var bitmapEntry))
                    return;

                bitmapEntry.Bitmap.Save(bitmapEntry.Path, ImageFormat.Png);
                bitmapEntry.Bitmap.Dispose();
            }
        }

        public async void StartWriting()
        {
            if (!isReadyToStart)
            {
                throw new Exception("Not ready to start! You should await ForceFinishWriting to learn when we can restart.");
            }

            isReadyToStart = false;
            isWritingNewBitmaps = true;

            await Task.Run(() =>
            {
                while (isWritingNewBitmaps)
                    Process();
            });
        }

        // Write the last remaining bitmaps and then stop.
        public async Task ForceFinishWriting()
        {
            isWritingNewBitmaps = false;

            await Task.Run(() =>
            {
                while (!bitmaps.IsEmpty)
                    Process();
            });

            isReadyToStart = true;
        }
    }
}
