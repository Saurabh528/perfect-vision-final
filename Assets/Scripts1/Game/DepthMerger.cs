
using System;
using UnityEngine;

public class DepthMerger
{
    public static Texture2D GenerateDepthImageFromWhiteImage(Texture2D sourceTtexture, int pixelDistance)
    {
        int width = sourceTtexture.width;
        int height = sourceTtexture.height;
        int num = width + Math.Abs(pixelDistance);
        Texture2D mergedTexture = new Texture2D(num, height);
        Color[] array = new Color[num * height];
        Color[] pixels = sourceTtexture.GetPixels();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < num; j++)
            {
                int num2;
                int num3;
                if (pixelDistance >= 0)
                {
                    num2 = j;
                    num3 = j - pixelDistance;
                }
                else
                {
                    num2 = j + pixelDistance;
                    num3 = j;
                }
                Color color;
                if (num2 >= 0 && num2 < width)
                {
                    color = pixels[i * width + num2];
                }
                else
                {
                    color = new Color(0f, 0f, 0f, 0f);
                }
                Color color2;
                if (num3 >= 0 && num3 < width)
                {
                    color2 = pixels[i * width + num3];
                }
                else
                {
                    color2 = new Color(0f, 0f, 0f, 0f);
                }
				//array[i * num + j] = new Color(color.a, color2.a, color2.a, (color.a > 0 || color2.a > 0)? 1f: 0);//For red/cyan profile
				array[i * num + j] = new Color(color.a, 0, color2.a, (color.a > 0 || color2.a > 0)? 1f: 0);//For red.blue profile
			}
		}
        mergedTexture.SetPixels(array);
        mergedTexture.Apply();
        return mergedTexture;
    }

	public static Texture2D GenerateDepthImageFromLeftRightImage(Texture2D leftTtexture, Texture2D rightTexture, int pixelDistance)
	{
		int width = leftTtexture.width;
		int height = leftTtexture.height;
		int num = width + Math.Abs(pixelDistance);
		Texture2D mergedTexture = new Texture2D(num, height);
		Color[] array = new Color[num * height];
		Color[] pixelsLeft = leftTtexture.GetPixels();
		Color[] pixelsRight = rightTexture.GetPixels();
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num2;
				int num3;
				if (pixelDistance >= 0)
				{
					num2 = j;
					num3 = j - pixelDistance;
				}
				else
				{
					num2 = j + pixelDistance;
					num3 = j;
				}
				Color color;
				if (num2 >= 0 && num2 < width)
				{
					color = pixelsLeft[i * width + num2];
				}
				else
				{
					color = new Color(0f, 0f, 0f, 0f);
				}
				Color color2;
				if (num3 >= 0 && num3 < width)
				{
					color2 = pixelsRight[i * width + num3];
				}
				else
				{
					color2 = new Color(0f, 0f, 0f, 0f);
				}
				
				//array[i * num + j] = new Color(color.a, color2.a, color2.a, (color.a > 0 || color2.a > 0)? 1f: 0);//For red/cyan profile
				array[i * num + j] = new Color(color.r * color.a + color2.r * color2.a, 
					color.g * color.a + color2.g * color2.a,
					color.b * color.a + color2.b * color2.a, (color.a > 0 || color2.a > 0) ? 1f : 0);//For red.blue profile
			}
		}
		mergedTexture.SetPixels(array);
		mergedTexture.Apply();
		return mergedTexture;
	}

	public static Texture2D GenerateRandomDotFromShape(Texture2D template, Texture2D shape, int depthPixel){


        int width = template.width;
        int height = template.height;
        Texture2D mergedTexture = new Texture2D(width, height);
        Color[] array = new Color[width * height];
        Color[] pixels = template.GetPixels();
        Color[] shapepixels = shape.GetPixels();
		for (int x = 0; x < width; x++){
            for(int y = 0; y < height; y++){
                Color color = pixels[y * width + x];
				if (shapepixels[y * width + x].r == 0)
                    array[y * width + x] = new Color(color.a, color.a, color.a, 1);
                else
				{
					array[y * width + x] = (x + depthPixel >= 1 && x + depthPixel < width - 1)? new Color(pixels[y * width + x + depthPixel].a, color.a, color.a, 1): color;
				}

			}
        }
        mergedTexture.SetPixels(array);
        mergedTexture.Apply();
        return mergedTexture;
    }

	public static void GenerateRandomDotChannelFromShape(Texture2D template, Texture2D shape, int depthPixel, bool BaseIn, out Texture2D leftChannel, out Texture2D rightChannel)
	{


		int width = template.width;
		int height = template.height;
		leftChannel = new Texture2D(width, height);
		rightChannel = new Texture2D(width, height);
		Color[] array = new Color[width * height];
		Color[] pixels = template.GetPixels();
		Color[] shapepixels = shape.GetPixels();
        if (BaseIn)
        {
            //left is blue, right is red(not using pattern)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = pixels[y * width + x];
                    array[y * width + x] = new Color(1, 0, 0, color.a);
                }
            }
            rightChannel.SetPixels(array);
            rightChannel.Apply();

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Color color = pixels[y * width + x];
					if (shapepixels[y * width + x].r == 0)
						array[y * width + x] = new Color(0, 0, 1, color.a);
					else
					{
						array[y * width + x] = (x + depthPixel >= 0 && x + depthPixel < width) ? new Color(0, 0, 1, pixels[y * width + x + depthPixel].a) : new Color(0, 0, 1, color.a); ;
					}
				}
			}
			leftChannel.SetPixels(array);
			leftChannel.Apply();
		}
		else
		{
			//left is red, right is blue(not using pattern)
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Color color = pixels[y * width + x];
					array[y * width + x] = new Color(0, 0, 1, color.a);
				}
			}
			rightChannel.SetPixels(array);
			rightChannel.Apply();

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					Color color = pixels[y * width + x];
					if (shapepixels[y * width + x].r == 0)
						array[y * width + x] = new Color(1, 0, 0, color.a);
					else
					{
						array[y * width + x] = (x + depthPixel >= 0 && x + depthPixel < width) ? new Color(1, 0, 0, pixels[y * width + x + depthPixel].a) : new Color(1, 0, 0, color.a); ;
					}
				}
			}
			leftChannel.SetPixels(array);
			leftChannel.Apply();
		}
	}

	public static void InsertMarkIntoTexture2D(Texture2D texture, int x, int y,
		Texture2D mark, ColorChannel colorChannel, out Texture2D textureOut)
	{
		int width = texture.width;
		int height = texture.height;
		int markwidth = mark.width;
		int markheight = mark.height;

		Color[] pixels = texture.GetPixels();
		Color[] markpixels = mark.GetPixels();
		for (int i = 0; i < markwidth; i++)
		{
			for (int j = 0; j < markheight; j++)
			{
				Color markColor = markpixels[j * markwidth + i];
				if(colorChannel == ColorChannel.CC_Red)
				{
					markColor.g = markColor.b = 0;
				}
				else if (colorChannel == ColorChannel.CC_Cyan)
				{
					markColor.r = markColor.g = 0;
				}
				pixels[width * (y + j) + x + i] = markColor;
			}
		}

		textureOut = new Texture2D(width, height);
		textureOut.SetPixels(pixels);
		textureOut.Apply();
	}
}
