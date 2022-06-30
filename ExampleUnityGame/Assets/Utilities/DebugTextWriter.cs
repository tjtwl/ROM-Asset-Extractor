using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ExampleUnityGame
{
    public class DebugTextWriter : TextWriter
    {
        private readonly StringBuilder output = new StringBuilder();

        public DebugTextWriter()
        {
        }

        public override void Write(char value)
        {
            output.Append(value);
        }

        public override void Write(string value)
        {
            output.Append(value);
        }

        public override void Flush()
        {
            base.Flush();
            Debug.Log(output);
            output.Clear();
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
