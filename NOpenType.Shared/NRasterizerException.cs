using System;

namespace NRasterizer
{
    public class NRasterizerException: Exception
    {
        public NRasterizerException()
        {
        }

        public NRasterizerException(string message)
            : base(message)
        {
        }

        public NRasterizerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
