import numpy as np
from PIL import Image

# Parameters
width, height = 800, 600  # Image dimensions
dot_density = 0.5         # Proportion of image filled with dots
disparity = 10            # Horizontal pixel shift for 3D effect

# Create random dot pattern
random_dots = (np.random.rand(height, width) < dot_density).astype(np.uint8) * 255

# Create a disparity map (e.g., a centered square)
disparity_map = np.zeros((height, width), dtype=np.int32)
square_size = 200
square_x = (width - square_size) // 2
square_y = (height - square_size) // 2
disparity_map[square_y:square_y + square_size, square_x:square_x + square_size] = disparity

# Generate left-eye and right-eye images
left_eye = random_dots.copy()
right_eye = random_dots.copy()  # Initialize with random dots to avoid blank areas

# Apply disparity map to right-eye image
for y in range(height):
    for x in range(width):
        shifted_x = x + disparity_map[y, x]
        if 0 <= shifted_x < width:
            right_eye[y, shifted_x] = left_eye[y, x]

# Save left-eye and right-eye images separately
left_eye_image = Image.fromarray(left_eye)
left_eye_image.save("left_eye_image.png")

right_eye_image = Image.fromarray(right_eye)
right_eye_image.save("right_eye_image.png")

# Combine into a stereogram
stereogram = np.zeros((height, width, 3), dtype=np.uint8)
stereogram[..., 0] = left_eye  # Red channel
stereogram[..., 1] = right_eye  # Green channel

# Save the combined stereogram
stereogram_image = Image.fromarray(stereogram)
stereogram_image.save("stereogram_image.png")

# Display paths to saved files
print("Saved files:")
print("- Left eye image: left_eye_image.png")
print("- Right eye image: right_eye_image.png")
print("- Combined stereogram: stereogram_image.png")

# Optionally, display all images
left_eye_image.show(title="Left Eye")
right_eye_image.show(title="Right Eye")
stereogram_image.show(title="Stereogram")
