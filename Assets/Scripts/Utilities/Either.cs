using System;

namespace Utilities
{
    public class Either<TL, TR>
    {
        private readonly TL _left;
        private readonly TR _right;
        private readonly bool _isLeft;

        public Either(TL left)
        {
            _left = left;
            _isLeft = true;
        }

        public Either(TR right)
        {
            _right = right;
            _isLeft = false;
        }

        public bool IsLeft => _isLeft;

        public TL Left => _left;
        public TR Right => _right;

        public void Match(Action<TL> leftCallback, Action<TR> rightCallback)
        {
            if (_isLeft)
            {
                leftCallback(_left);
            }
            else
            {
                rightCallback(_right);
            }
        }

        public static implicit operator Either<TL, TR>(TL left) => new Either<TL, TR>(left);

        public static implicit operator Either<TL, TR>(TR right) => new Either<TL, TR>(right);
    }
}