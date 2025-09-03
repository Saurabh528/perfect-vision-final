import pygame
import numpy as np
import random
import time
# Initialize Pygame
pygame.init()
# Screen dimensions
SCREEN_WIDTH = 1200
SCREEN_HEIGHT = 800
# Colors
GRAY = (128, 128, 128)
BLACK = (0, 0, 0)
WHITE = (255, 255, 255)
# Therapy duration
SESSION_LENGTH = 180  # 3 minutes in seconds
def create_gabor_patch(size=128, wavelength=25, orientation=0, phase=0, sigma=40, amplitude=1.0):
    """
    Create a Gabor patch with fully natural boundaries, blending into the background.
    """
    orientation_rad = np.deg2rad(orientation)
    phase_rad = np.deg2rad(phase)
    x = np.linspace(-size/2, size/2, size)
    y = np.linspace(-size/2, size/2, size)
    X, Y = np.meshgrid(x, y)
    # Rotate coordinates
    X_rot = X * np.cos(orientation_rad) + Y * np.sin(orientation_rad)
    gaussian = np.exp(-(X**2 + Y**2) / (2 * sigma**2))
    carrier = np.sin(2 * np.pi * X_rot / wavelength + phase_rad)
    gabor = amplitude * carrier * gaussian
    # Normalize Gabor values to 0–255
    gabor = ((gabor - np.min(gabor)) / (np.max(gabor) - np.min(gabor)) * 255).astype(np.uint8)
    # Apply Gaussian transparency (alpha channel) for natural fading
    alpha_channel = (gaussian * 255).astype(np.uint8)
    # Create a Pygame surface with per-pixel alpha
    surface = pygame.Surface((size, size), pygame.SRCALPHA)
    # Draw Gabor patch with transparency
    for y in range(size):
        for x in range(size):
            color = gabor[y, x]
            alpha = alpha_channel[y, x]
            if alpha > 0:  # Only draw pixels with non-zero transparency
                surface.set_at((x, y), (color, color, color, alpha))
    return surface
def rotate_surface(surface, angle):
    """Rotate a Pygame surface."""
    return pygame.transform.rotate(surface, angle)
def draw_quit_button(screen):
    """Draw a Quit button on the screen."""
    quit_button_rect = pygame.Rect(SCREEN_WIDTH - 150, 20, 100, 40)
    pygame.draw.rect(screen, WHITE, quit_button_rect)
    pygame.draw.rect(screen, BLACK, quit_button_rect, 2)
    font = pygame.font.SysFont(None, 24)
    quit_text = font.render("Quit", True, BLACK)
    quit_text_rect = quit_text.get_rect(center=quit_button_rect.center)
    screen.blit(quit_text, quit_text_rect)
    return quit_button_rect
def adjust_level_parameters(level):
    """
    Adjust gameplay parameters based on the current level.
    """
    params = {
        "wavelength_target": max(15, 30 - level * 2),  # Target wavelength decreases at higher levels
        "wavelength_distractor_range": (max(15, 30 - level * 3), 40),  # Distractors have a broader range
        "contrast": max(0.3, 1.0 - level * 0.1),  # Decrease contrast gradually
        "rotation_speed": 0.2 + level * 0.1,  # Increase rotation speed
        "grid_rows": min(1 + level, 6),  # Increase rows up to 6
        "grid_cols": min(1 + level, 8),  # Increase columns up to 8
        "target_count": min(1 + level, 8),  # Increase targets up to 8
    }
    return params
def dichoptic_attention_task():
    """Run the Gabor training task with levels."""
    screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
    pygame.display.set_caption("Gabor Training Game with Natural Boundaries")
    clock = pygame.time.Clock()
    level = 1
    score = 0
    correct_responses = 0
    incorrect_responses = 0
    total_images = 0
    start_time = pygame.time.get_ticks()
    response_times = []
    running = True
    quit_game = False  # New flag to handle quitting
    while running and not quit_game:
        elapsed_time = (pygame.time.get_ticks() - start_time) // 1000
        if elapsed_time >= SESSION_LENGTH:
            running = False
            break
        params = adjust_level_parameters(level)
        grid_rows = params["grid_rows"]
        grid_cols = params["grid_cols"]
        target_count = params["target_count"]
        wavelength_target = params["wavelength_target"]
        wavelength_distractor_range = params["wavelength_distractor_range"]
        contrast = params["contrast"]
        rotation_speed = params["rotation_speed"]
        patch_size = min((SCREEN_WIDTH - 300) // grid_cols, (SCREEN_HEIGHT - 400) // grid_rows)
        spacing_x = (SCREEN_WIDTH - (grid_cols * patch_size)) // (grid_cols + 1)
        spacing_y = (SCREEN_HEIGHT - (grid_rows * patch_size) - 300) // (grid_rows + 1)
        grid_offset_x = spacing_x
        grid_offset_y = 200 + spacing_y
        # Generate patches
        patches = []
        target_indices = random.sample(range(grid_rows * grid_cols), target_count)
        base_theta = random.uniform(0, 360)  # Target orientation
        for i in range(grid_rows * grid_cols):
            if i in target_indices:
                # Target patch
                target_patch = create_gabor_patch(
                    size=patch_size, wavelength=wavelength_target, orientation=base_theta, sigma=40, amplitude=contrast
                )
                patches.append((target_patch, True))
            else:
                # Distractor patch with distinct configurations
                distractor_theta = random.uniform(0, 360)
                distractor_wavelength = random.uniform(10, 50)  # Wider range for spatial frequency
                distractor_sigma = random.uniform(30, 50)  # Adjust envelope size
                distractor_amplitude = random.uniform(0.5, 0.8)  # Lower contrast for distractors
                distractor_patch = create_gabor_patch(
                    size=patch_size, wavelength=distractor_wavelength, orientation=distractor_theta,
                    sigma=distractor_sigma, amplitude=distractor_amplitude
                )
                patches.append((distractor_patch, False))
        # Reference patch (static)
        reference_patch = create_gabor_patch(size=patch_size, wavelength=wavelength_target, orientation=base_theta, amplitude=contrast)
        rotation_angles = [random.randint(0, 360) for _ in range(len(patches))]
        start_level_time = time.time()
        correct_clicks = 0
        while correct_clicks < target_count and not quit_game:  # Added quit_game check
            screen.fill(GRAY)
            # Draw UI elements
            font = pygame.font.SysFont(None, 36)
            level_text = font.render(f"Level: {level}", True, BLACK)
            score_text = font.render(f"Score: {score}", True, BLACK)
            time_text = font.render(f"Time Left: {SESSION_LENGTH - elapsed_time}s", True, BLACK)
            screen.blit(level_text, (20, 20))
            screen.blit(score_text, (20, 60))
            screen.blit(time_text, (20, 100))
            # Display reference patch
            target_label = font.render("Target Patch (Reference):", True, BLACK)
            label_x = (SCREEN_WIDTH - target_label.get_width()) // 2
            screen.blit(target_label, (label_x, 50))
            reference_x = (SCREEN_WIDTH - patch_size) // 2
            screen.blit(reference_patch, (reference_x, 100))
            # Draw patches
            for idx, patch_data in enumerate(patches):
                if patch_data is None:
                    continue
                patch, is_target = patch_data
                col = idx % grid_cols
                row = idx // grid_cols
                x = col * (patch_size + spacing_x) + grid_offset_x
                y = row * (patch_size + spacing_y) + grid_offset_y
                rotated_patch = rotate_surface(patch, rotation_angles[idx])
                screen.blit(rotated_patch, (x, y))
                rotation_angles[idx] += rotation_speed
            # Draw Quit button
            quit_button_rect = draw_quit_button(screen)
            pygame.display.flip()
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    quit_game = True
                    break
                if event.type == pygame.MOUSEBUTTONDOWN:
                    mouse_x, mouse_y = event.pos
                    if quit_button_rect.collidepoint(mouse_x, mouse_y):
                        quit_game = True
                        break
                    col = (mouse_x - grid_offset_x) // (patch_size + spacing_x)
                    row = (mouse_y - grid_offset_y) // (patch_size + spacing_y)
                    if 0 <= col < grid_cols and 0 <= row < grid_rows:
                        selected_index = row * grid_cols + col
                        if patches[selected_index] is not None:
                            patch, is_target = patches[selected_index]
                            if is_target:
                                score += 10
                                correct_responses += 1
                                correct_clicks += 1
                                patches[selected_index] = None
                            else:
                                score -= 5
                                incorrect_responses += 1
            if quit_game:
                break
        if quit_game:
            break
        response_time = time.time() - start_level_time
        response_times.append(response_time)
        total_images += 1
        # Adjust level based on score
        if score > 30:
            level += 1
        elif score < -20:
            level = max(1, level - 1)
    # Display final report
    screen.fill(GRAY)
    avg_response_time = np.mean(response_times) if response_times else 0
    improvement_percentage = (level - 1) * 10
    report = [
        f"Final Level: {level}",
        f"Final Score: {score}",
        f"Correct Responses: {correct_responses}",
        f"Incorrect Responses: {incorrect_responses}",
        f"Avg Response Time: {avg_response_time:.2f}s",
        f"Improvement: {improvement_percentage}%"
    ]
    for i, line in enumerate(report):
        text = pygame.font.SysFont(None, 36).render(line, True, BLACK)
        screen.blit(text, (20, 100 + i * 50))
    pygame.display.flip()
    pygame.time.wait(5000)
if __name__ == "__main__":
    dichoptic_attention_task()
    pygame.quit()