
import cv2
import numpy as np
import os

def process_image(input_image_path):
    # Load the image with OpenCV
    img = cv2.imread(input_image_path, cv2.IMREAD_COLOR)  # Loads the image in BGR format

    if img is None:
        raise FileNotFoundError(f"Image not found: {input_image_path}")

    """
    # method 1
    # Convert the image from BGR to RGB (OpenCV uses BGR by default)
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    # Compute the alpha channel based on the maximum value among R, G, and B
    max_rgb = np.max(img_rgb, axis=2)  # Get maximum RGB value for each pixel
    alpha_channel = max_rgb  # Use this as the alpha channel

    # Stack the RGB image and alpha channel to create an RGBA image
    img_rgba = np.dstack((img_rgb, alpha_channel))
    # Generate the output file name by appending '_converted' before the file extension
    base, ext = os.path.splitext(input_image_path)
    output_image_path = f"{base}_converted{ext}"

    # Save the new image (converting RGBA to BGRA for OpenCV saving)
    cv2.imwrite(output_image_path, cv2.cvtColor(img_rgba, cv2.COLOR_RGBA2BGRA))

    """

    # method 2
    # Convert the image from BGR to RGB (OpenCV uses BGR by default)
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    # get half
    img_rgb = img_rgb[:, :(int)(img_rgb.shape[1] / 2)] #left half
    # img_rgb = img_rgb[:, (int)(img_rgb.shape[1] / 2):] #right half
    alpha_channel = np.ones((img_rgb.shape[0], img_rgb.shape[1]), dtype=img_rgb.dtype) * 255
    # Merge the RGB image with the alpha channel
    rgba_image = cv2.merge((img_rgb, alpha_channel))
    for y in range(rgba_image.shape[0]):  # Loop over rows
        for x in range(rgba_image.shape[1]):  # Loop over columns
            r, g, b, a = rgba_image[y, x]
            
            # Modify the values as needed
            # For red image
            rgba_image[y, x] = [255, 0, 0, r]

            # For green image
            """bNew = 0
            if g != 0 and b != 0:
                    bNew = (int)(255.0 * b / g)
            rgba_image[y, x] = [0, 255, bNew , g]"""

            # For blue image
            """gNew = 0
            if g != 0 and b != 0:
                    gNew = (int)(255.0 * g / b)
            rgba_image[y, x] = [0, gNew, 255 , b]"""

    # Generate the output file name by appending '_converted' before the file extension
    base, ext = os.path.splitext(input_image_path)
    output_image_path = f"{base}_converted{ext}"

    # Save the new image (converting RGBA to BGRA for OpenCV saving)
    cv2.imwrite(output_image_path, cv2.cvtColor(rgba_image, cv2.COLOR_RGBA2BGRA))

    print(f"Image saved as {output_image_path}")

# Example usage
input_image_path = 'f:/temp/RandomDotTest/VisualPowerBlue/Screenshot_11.png'  # Replace with your image path
process_image(input_image_path)
