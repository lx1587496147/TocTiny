using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TocTinyWPF.Package
{
    public interface IBinaryPackage
    {
        byte[] ToBytes();
    }
    public interface ITextPackage
    {
        string ToString();
    }
    public abstract class TextPackage : IBinaryPackage, ITextPackage
    {
        public Encoding Encoding { get; protected set; } = Encoding.UTF8;
        public byte[] ToBytes()
        {
            return Encoding.GetBytes(ToString());
        }

        public override abstract string ToString();

    }
}
