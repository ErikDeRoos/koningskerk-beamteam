using System;

namespace mppt.Connect
{
    public interface IMppFactory
    {
        IMppApplication GetApplication();
        IMppShapeTableContent GetMppShapeTableContent1Column(int index, string column1, bool mergeRemainingColumns);
        IMppShapeTableContent GetMppShapeTableContent3Column(int index, string column1, string column2, string column3);
    }

    public class MppFactory : IMppFactory
    {
        private Func<IMppApplication> _applicationGetter;
        public MppFactory(Func<IMppApplication> applicationGetter)
        {
            _applicationGetter = applicationGetter;
        }

        public IMppApplication GetApplication()
        {
            return _applicationGetter();
        }

        public IMppShapeTableContent GetMppShapeTableContent1Column(int index, string column1, bool mergeRemainingColumns)
        {
            return new MppShapeTableContent1Column(index, column1, mergeRemainingColumns);
        }

        public IMppShapeTableContent GetMppShapeTableContent3Column(int index, string column1, string column2, string column3)
        {
            return new MppShapeTableContent3Column(index, column1, column2, column3);
        }
    }
}
