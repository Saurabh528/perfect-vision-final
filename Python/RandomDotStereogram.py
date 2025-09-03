import numpy as np
import pygame
from pygame.locals import *
import math
import time
def generate_random_dots(width, height):
    """
    Generate a random dot pattern for the background.
    """
    return np.random.randint(0, 2, (height, width)) * 255
def apply_disparity(dots, shape_mask, disparity):
    """
    Apply horizontal disparity to the shape within the random dot pattern.
    """
    height, width = dots.shape
    left_eye = np.copy(dots)
    right_eye = np.copy(dots)
    # Apply disparity to shape region only
    shape_indices = np.where(shape_mask)
    for y, x in zip(*shape_indices):
        if 0 <= x - disparity < width:
            left_eye[y, x] = dots[y, x - disparity]
        if 0 <= x + disparity < width:
            right_eye[y, x] = dots[y, x + disparity]
    return left_eye, right_eye
def create_shape_mask(width, height, shape='square', shape_size=50, x_center=None, y_center=None):
    """
    Create a mask for the shape to be embedded into the random dot pattern.
    """
    mask = np.zeros((height, width), dtype=bool)
    if x_center is None:
        x_center = width // 2
    if y_center is None:
        y_center = height // 2
    if shape == 'square':
        x_start = max(0, x_center - shape_size // 2)
        x_end = min(width, x_center + shape_size // 2)
        y_start = max(0, y_center - shape_size // 2)
        y_end = min(height, y_center + shape_size // 2)
        mask[y_start:y_end, x_start:x_end] = True
    elif shape == 'circle':
        y_indices, x_indices = np.ogrid[:height, :width]
        dist_from_center = np.sqrt((x_indices - x_center) ** 2 + (y_indices - y_center) ** 2)
        mask[dist_from_center <= shape_size // 2] = True
    return mask
def display_stereogram(screen_size, viewing_distance, screen_ppi):
    """
    Display stereogram for true 3D effect with left and right images.
    """
    pygame.init()
    screen = pygame.display.set_mode(screen_size)
    pygame.display.set_caption("Random Dot Stereogram - True 3D Effect")
    font = pygame.font.Font(None, 36)
    start_text = font.render("Press SPACE to Start", True, (255, 255, 255))
    quit_text = font.render("Press Q to Quit", True, (255, 255, 255))
    running = True
    started = False
    clock = pygame.time.Clock()
    # Parameters
    screen_width, screen_height = screen_size
    disparity = 10  # Initial disparity in pixels
    shape_size = 100  # Shape size in pixels
    shape = 'square'  # Initial shape
    random_dots = generate_random_dots(screen_width, screen_height)
    shape_mask = create_shape_mask(screen_width, screen_height, shape, shape_size)
    left_eye, right_eye = apply_disparity(random_dots, shape_mask, disparity)
    while running:
        for event in pygame.event.get():
            if event.type == QUIT:
                running = False
            if event.type == KEYDOWN:
                if event.key == K_SPACE:
                    started = True
                if event.key == K_q:
                    running = False
                if started and event.key in [K_s, K_c]:
                    # Adjust disparity and shape
                    if event.key == K_s and shape == 'square':
                        disparity = max(1, disparity - 1)  # Make tougher
                    else:
                        disparity = min(20, disparity + 1)  # Make easier
                    shape = 'circle' if shape == 'square' else 'square'
                    shape_mask = create_shape_mask(screen_width, screen_height, shape, shape_size)
                    left_eye, right_eye = apply_disparity(random_dots, shape_mask, disparity)
        screen.fill((0, 0, 0))
        if not started:
            screen.blit(start_text, (screen_width // 2 - start_text.get_width() // 2, screen_height // 2 - 50))
            screen.blit(quit_text, (screen_width // 2 - quit_text.get_width() // 2, screen_height // 2 + 10))
        else:
            # Alternate left and right images for stereoscopic 3D effect
            if time.time() % 0.1 < 0.05:  # Alternate every ~50ms
                stereogram_surface = pygame.surfarray.make_surface(left_eye)
            else:
                stereogram_surface = pygame.surfarray.make_surface(right_eye)
            screen.blit(stereogram_surface, (0, 0))
        pygame.display.flip()
        clock.tick(60)
    pygame.quit()
def main():
    screen_width = 800
    screen_height = 600
    viewing_distance = 50  # Viewing distance in cm
    screen_ppi = 130.9  # Pixels per inch
    display_stereogram((screen_width, screen_height), viewing_distance, screen_ppi)
if __name__ == "__main__":
    main()