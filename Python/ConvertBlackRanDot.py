
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
    # img_rgb = img_rgb[:, :(int)(img_rgb.shape[1] / 2)] #left half
    # img_rgb = img_rgb[:, (int)(img_rgb.shape[1] / 2):] #right half
    alpha_channel = np.ones((img_rgb.shape[0], img_rgb.shape[1]), dtype=img_rgb.dtype) * 255
    # Merge the RGB image with the alpha channel
    rgba_image = cv2.merge((img_rgb, alpha_channel))
    for y in range(rgba_image.shape[0]):  # Loop over rows
        for x in range(rgba_image.shape[1]):  # Loop over columns
            r, g, b, a = rgba_image[y, x]
            
            #Convert blue image to grey image
            rgba_image[y, x] = [255, 255, 255, b]

            # Modify the values as needed
            # For white image
            """ if r == g and r == b:
                rgba_image[y, x] = [255, 255, 255, r]
            else:
                rgba_image[y, x] = [255, 255, 255, 0] """
            # For red and blue image
            """ if x > 953:
                rgba_image[y, x] = [0, 255, 255, g]
            else:
                rgba_image[y, x] = [255, 0, 0, r] """

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

def convertBlueToGrey(input_image_path):
    # Load the image with OpenCV
    img = cv2.imread(input_image_path, cv2.IMREAD_UNCHANGED)  # Loads the image in BGR format

    if img is None:
        raise FileNotFoundError(f"Image not found: {input_image_path}")

    for y in range(img.shape[0]):  # Loop over rows
        for x in range(img.shape[1]):  # Loop over columns
            b, g, r, a = img[y, x]
            
            #Convert blue image to grey image
            img[y, x] = [255, 255, 255, a]


    # Generate the output file name by appending '_converted' before the file extension
    base, ext = os.path.splitext(input_image_path)
    output_image_path = f"{base}_converted{ext}"

    # Save the new image (converting RGBA to BGRA for OpenCV saving)
    cv2.imwrite(output_image_path, img)

    print(f"Image saved as {output_image_path}")

def create_random_dot_image_with_alpha_phases(width, height, white_prob, black_prob, output_file=None):
    """
    Creates a random dot image with 1-pixel dots and varying transparency using phase iteration.
    
    Parameters:
        width (int): Width of the image.
        height (int): Height of the image.
        transparency_prob (float): Probability (0 to 1) that a dot will be semi-transparent.
        output_file (str, optional): Path to save the image. If None, the image is not saved.
    
    Returns:
        np.ndarray: The generated random dot image with alpha channel.
    """
    # Create a blank transparent image (RGBA)
    image = np.zeros((height, width, 4), dtype=np.uint8)
    
    # Iterate over all pixel positions
    for y in range(height):
        for x in range(width):
            # Determine if the dot will be semi-transparent
            alpha = 0
            if np.random.rand() > black_prob:
                alpha = 0
            elif np.random.rand() > white_prob:
                alpha = 255
            else:
                alpha = np.random.randint(200, np.random.randint(240, 255))
            color = (255, 255, 255, alpha)  # White color with varying alpha
            
            # Place the pixel with the defined color
            image[y, x] = color
    
    # Save the image if an output path is provided
    if output_file:
        cv2.imwrite(output_file, image)
    
    return image
# Example usage

input_image_path = 'f:/temp/Screenshot 2025-01-04 221536.png'  # Replace with your image path
process_image(input_image_path)

#create random dot image
#create_random_dot_image_with_alpha_phases(512, 512, 0.8, 0.5, "F:/temp/randot.png")
