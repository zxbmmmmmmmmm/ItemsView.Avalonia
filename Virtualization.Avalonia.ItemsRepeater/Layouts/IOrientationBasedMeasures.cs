using Avalonia;

namespace Virtualization.Avalonia.Layouts;

internal interface IOrientationBasedMeasures
{
    ScrollOrientation ScrollOrientation { get; set; }
}

internal static class OrientationBasedMeasuresExt
{   
    extension(IOrientationBasedMeasures m)
    {
        /// <summary>
        /// The length of non-scrolling direction
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public double Major(Size size) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? size.Height : size.Width;

        /// <summary>
        /// The length of scrolling direction
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public double Minor(Size size) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? size.Width : size.Height;

        public double MajorSize(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.Height : rect.Width;

        public void SetMajorSize(ref Rect rect, double value) =>
            rect = m.ScrollOrientation is ScrollOrientation.Vertical ? rect.WithHeight(value) : rect.WithWidth(value);

        public double MinorSize(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.Width : rect.Height;

        public void SetMinorSize(ref Rect rect, double value) => 
            rect = m.ScrollOrientation is ScrollOrientation.Vertical ? rect.WithWidth(value) : rect.WithHeight(value);

        public double MajorStart(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.Y : rect.X;

        public double MajorEnd(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.Bottom : rect.Right;

        public double MinorStart(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.X : rect.Y;

        public void SetMinorStart(ref Rect rect, double value) =>
            rect = m.ScrollOrientation is ScrollOrientation.Vertical ? rect.WithX(value) : rect.WithY(value);

        public void SetMajorStart(ref Rect rect, double value) => 
            rect = m.ScrollOrientation is ScrollOrientation.Vertical ? rect.WithY(value) : rect.WithX(value);

        public double MinorEnd(Rect rect) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ? rect.Right : rect.Bottom;

        public Rect MinorMajorRect(double minor, double major, double minorSize, double majorSize) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ?
                new Rect(minor, major, minorSize, majorSize) :
                new Rect(major, minor, majorSize, minorSize);

        public Point MinorMajorPoint(double minor, double major) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ?
                new Point(minor, major) : new Point(major, minor);

        public Size MinorMajorSize(double minor, double major) =>
            m.ScrollOrientation is ScrollOrientation.Vertical ?
                new Size(minor, major) : new Size(major, minor);
    }
}
