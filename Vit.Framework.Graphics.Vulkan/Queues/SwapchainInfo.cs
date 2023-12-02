using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class SwapchainInfo {
	public QueueFamily GraphicsFamily = null!;
	public QueueFamily PresentFamily = null!;
	public VkSurfaceCapabilitiesKHR Capabilities;
	public VkSurfaceFormatKHR[] Formats = null!;
	public VkPresentModeKHR[] PresentModes = null!;

	public SwapchainFormat SelectBest () {
		return new() {
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

	static readonly CString VK_KHR_swapchain = CString.CreateStaticPinned( "VK_KHR_swapchain" );
	public static readonly IReadOnlyList<string> RequiredExtensions = new[] { "VK_KHR_swapchain" };
	public static readonly IReadOnlyList<CString> RequiredExtensionsCstr = new[] { VK_KHR_swapchain };
}

public struct SwapchainFormat {
	public VkSurfaceFormatKHR Format;
	public VkPresentModeKHR PresentMode;
	public uint OptimalImageCount;
}