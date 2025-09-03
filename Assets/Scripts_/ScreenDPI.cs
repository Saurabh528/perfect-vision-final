using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ScreenDPI
{
	[DllImport("User32.dll")]
	private static extern IntPtr GetDC(IntPtr hwnd);
	[DllImport("User32.dll")]
	private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
	[DllImport("Gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

	private const int HORZSIZE = 4; // Width in mm
	private const int VERTSIZE = 6; // Height in mm
	private const int HORZRES = 8; // Horizontal resolution
	private const int VERTRES = 10; // Vertical resolution
	static float dpi;

	public static float GetPPI()
	{
		if(dpi != 0)
			return dpi;
		IntPtr hdc = GetDC(IntPtr.Zero);
		float screenWidthMm = GetDeviceCaps(hdc, HORZSIZE);
		float screenHeightMm = GetDeviceCaps(hdc, VERTSIZE);
		float screenWidthPx = GetDeviceCaps(hdc, HORZRES);
		float screenHeightPx = GetDeviceCaps(hdc, VERTRES);
		ReleaseDC(IntPtr.Zero, hdc);

		float diagonalMm = Mathf.Sqrt(screenWidthMm * screenWidthMm + screenHeightMm * screenHeightMm);
		float diagonalInches = diagonalMm / 25.4f; // Convert mm to inches
		dpi = Mathf.Sqrt(screenWidthPx * screenWidthPx + screenHeightPx * screenHeightPx) / diagonalInches;
		return dpi;
	}
}