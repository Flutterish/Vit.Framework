using Vit.Framework.Interop;
using Vit.Framework.Windowing;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class SwapchainInfo {
	public QueueFamily GraphicsQueue = null!;
	public QueueFamily PresentQueue = null!;
	public VkSurfaceCapabilitiesKHR Capabilities;
	public VkSurfaceFormatKHR[] Formats = null!;
	public VkPresentModeKHR[] PresentModes = null!;

	public SwapchainFormat SelectBest () {
		return new() {
			Capabilities = Capabilities,
			Format = Formats.OrderBy( x => (x.format, x.colorSpace) switch {
				(VkFormat.B8g8r8a8Srgb, VkColorSpaceKHR.SrgbNonlinearKHR) => 1,
				_ => 2
			} ).First(),
			PresentMode = PresentModes.OrderBy( x => x switch {
				VkPresentModeKHR.MailboxKHR => 1,
				VkPresentModeKHR.FifoKHR => 2,
				VkPresentModeKHR.ImmediateKHR => 4,
				_ => 3
			} ).First(),
			OptimalImageCount = Math.Min( Capabilities.minImageCount + 1, Capabilities.maxImageCount == 0 ? uint.MaxValue : Capabilities.maxImageCount )
		};
	}

	public VkExtent2D GetSwapchainExtent ( Window window ) {
		if ( Capabilities.currentExtent.width != uint.MaxValue ) {
			return Capabilities.currentExtent;
		}

		return new() {
			width = (uint)Math.Clamp( window.PixelWidth, Capabilities.minImageExtent.width, Capabilities.maxImageExtent.width ),
			height = (uint)Math.Clamp( window.PixelHeight, Capabilities.minImageExtent.height, Capabilities.maxImageExtent.height )
		};
	}

	public static readonly IReadOnlyList<string> RequiredExtensions = new[] { "VK_KHR_swapchain" };
	public static readonly IReadOnlyList<CString> RequiredExtensionsCstr = RequiredExtensions.Select( x => (CString)x ).ToArray();
}

public struct SwapchainFormat {
	public VkSurfaceCapabilitiesKHR Capabilities;
	public VkSurfaceFormatKHR Format;
	public VkPresentModeKHR PresentMode;
	public uint OptimalImageCount;
}