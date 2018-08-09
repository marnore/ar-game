# Augmented reality game based on VimAI (vim.ai) technology, ARCore and Unity

Unity version used: 2018.1.0f2

<img src="preview.jpg" width="320"><img src="preview2.jpg" width="320">

# Game logic

Collect randomly spawned sport equipment, and energy bars. Sport equipmnet can be used in minigames or to collect more equipment. Items can be picked up and equipped from the inventory. Rewards are given when collecting items and by playing minigames.
Blue circle is energy indicator and green circle experience indicator.

Building is detected on startup based on GPS

Available games list is shown when location is detected from image


# Controls

Phone location and orientation for movement, and touch screen for using equipped item or interacting with items in world.


## Debug Menu Items

Restart App - Reload scene completely

Select Building - Select a building manually

Dummy Location - Get a random position in the world (as detected from image)

Test Image - Send random image from /Android/data/[package name]/files/test/ folder to location server (must be created manually)