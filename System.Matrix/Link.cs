using System;

namespace MV_Client
{
    public abstract class Link
    {
        public abstract void Open();
    }
    /// <summary>
    /// 通道上行
    /// </summary>
    public class UpLink : Link
    {
        public override void Open()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 通道下行
    /// </summary>
    public class DownLink : Link
    {
        public override void Open()
        {
            throw new NotImplementedException();
        }
    }

    public class LinkFactory
    {
        public static Link GetLink(UpDown upDown)
        {
            switch (upDown)
            {
                case UpDown.UP:
                    return new UpLink();
                case UpDown.DOWN:
                    return new DownLink();
                default:
                    return null;
            }
        }
    }

    public enum UpDown
    {
        UP,
        DOWN
    }
}
