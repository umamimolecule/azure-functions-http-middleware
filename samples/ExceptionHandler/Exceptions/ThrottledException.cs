using System;

namespace Samples.ModelValidation.Exceptions
{
    class ThrottledException : Exception
    {
        public ThrottledException(TimeSpan tryAgain)
        {
            this.TryAgain = tryAgain;
        }

        public TimeSpan TryAgain { get; private set; }
    }
}
