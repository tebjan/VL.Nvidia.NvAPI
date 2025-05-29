// In NvAPIWrapper.Native.GSyncApi class

using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GSync.Enums;
using NvAPIWrapper.Native.GSync.Structures;

namespace NvApi.GSync;

/// <summary>
/// Represents a GSync device with its handle and essential capabilities.
/// An instance is only created if its capabilities can be successfully fetched.
/// Other parameters can be queried on demand.
/// </summary>
public class GSyncDevice
{
    /// <summary>
    /// Gets the native handle to the GSync device.
    /// </summary>
    public IntPtr Handle { get; }

    /// <summary>
    /// Gets the capabilities of the GSync device.
    /// This is populated during successful creation of the instance.
    /// </summary>
    public GSyncCapabilitiesV3 Capabilities { get; }

    // Private constructor to force creation via TryCreate
    private GSyncDevice(IntPtr handle, GSyncCapabilitiesV3 capabilities)
    {
        Handle = handle;
        Capabilities = capabilities;
    }

    /// <summary>
    /// Attempts to create a GSyncDevice instance by fetching its essential capabilities.
    /// </summary>
    /// <param name="handle">The native handle to the GSync device.</param>
    /// <param name="device">When this method returns, contains the created GSyncDevice
    /// object if the capabilities were successfully fetched; otherwise, null.</param>
    /// <returns>True if the GSyncDevice was successfully created; otherwise, false.</returns>
    public static bool TryCreate(IntPtr handle, out GSyncDevice device)
    {
        device = null;
        try
        {

            GSyncApi.GSyncQueryCapabilities(handle, out var caps);

            // If GSyncQueryCapabilities succeeds, create the GSyncDevice
            device = new GSyncDevice(handle, caps);
            return true;
        }
        catch (NVIDIAApiException)
        {
            // Failed to get capabilities for this device.
            return false;
        }
        catch (Exception)
        {
            // Other unexpected errors during struct init or call.
            return false;
        }
    }

    // --- On-Demand Query Methods (remain the same) ---

    public GSyncControlParametersV2 GetControlParameters()
    {
        GSyncApi.GSyncGetControlParameters(Handle, out var controlParams);
        return controlParams;
    }

    public GSyncStatusParametersV2 GetStatusParameters()
    {
        GSyncApi.GSyncGetStatusParameters(Handle, out var statusParams);
        return statusParams;
    }

    public GSyncBoardStatus GetBoardStatus()
    {
        GSyncApi.GSyncGetSyncStatus(Handle, out var boardStatus);
        return boardStatus;
    }

    public void GetTopology(out GSyncGpuV2[] gsyncGPUs, out GSyncDisplayV2[] gsyncDisplays)
    {
        GSyncApi.GSyncGetTopology(Handle, out gsyncGPUs, out gsyncDisplays);
    }

    public void SetControlParameters(ref GSyncControlParametersV2 gsyncControls)
    {
        GSyncApi.GSyncSetControlParameters(Handle, ref gsyncControls);
    }

    public void AdjustSyncDelay(GSyncDelayType delayType, ref GSyncDelay gsyncDelay, out uint syncSteps)
    {
        GSyncApi.GSyncAdjustSyncDelay(Handle, delayType, ref gsyncDelay, out syncSteps);
    }

    public void AdjustSyncDelay(GSyncDelayType delayType, ref GSyncDelay gsyncDelay)
    {
        uint dummy;
        GSyncApi.GSyncAdjustSyncDelay(Handle, delayType, ref gsyncDelay, out dummy);
    }

    public override string ToString()
    {
        return $"GSync Device (Handle: {Handle}, BoardID: {Capabilities.BoardId})";
    }
}

public static class GSyncUtils
{
    /// <summary>
    /// Enumerates GSync devices for which essential capabilities can be successfully retrieved.
    /// </summary>
    /// <returns>A list of valid GSyncDevice objects. If capability fetching fails for a device handle, it's omitted.</returns>
    /// <exception cref="NVIDIAApiException">Thrown if the initial enumeration of device handles itself fails (e.g., NVAPI not found).</exception>
    public static List<GSyncDevice> EnumerateGSyncDevices()
    {
        var validGSyncDevices = new List<GSyncDevice>();

        IntPtr[] deviceHandles;
        uint deviceCount;

        GSyncApi.GSyncEnumSyncDevices(out deviceHandles, out deviceCount); // This can throw

        if (deviceCount == 0)
        {
            return validGSyncDevices;
        }

        for (int i = 0; i < deviceCount; i++)
        {
            IntPtr handle = deviceHandles[i];
            GSyncDevice deviceInstance;
            if (GSyncDevice.TryCreate(handle, out deviceInstance))
            {
                validGSyncDevices.Add(deviceInstance);
            }
            // If TryCreate returns false, we simply don't add it to the list.
        }

        return validGSyncDevices;
    }
}