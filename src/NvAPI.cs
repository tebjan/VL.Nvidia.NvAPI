// For examples, see:
// https://thegraybook.vvvv.org/reference/extending/writing-nodes.html#examples
using NvAPIWrapper;
namespace VL.Nvidia.NvAPI;

[ProcessNode]
public class NvAPI : IDisposable
{
    public NvAPI()
    {
        NVIDIA.Initialize();
    }

    //finalizer
    ~NvAPI()
    {
        Dispose();
    }

    public void Dispose()
    {
        NVIDIA.Unload();
    }
}