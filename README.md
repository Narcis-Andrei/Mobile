## Project Overview
- This project is a cross platform mobile game developed in Unity. The game is an vampiric survival based experience focused on mobile first interaction, gesture controls, and progressive difficulty scaling
- The player survives for as long as possible while enemies continuously spawn and increase in difficulty
- The player automatically attacks enemies and collects upgrades from chests to grow stronger over time
- The project has been designed and built for Web, Android, and iOS, with platform specific features enabled where appropriate


## Core Game Loop
1. The player moves using an on screen joystick
2. Enemies spawn continuously and pursue the player
3. The player automatically fires projectiles at nearby enemies
4. Chests spawn around the map
5. The player opens chests to select upgrades
6. Difficulty increases over time while the player also becomes stronger
7. The run ends on player death and updates persistent statistics


### Controls and Interactions v

## Menu Interaction
- In the main menu, gyroscope input is active on mobile devices
- Tilting the device allows the player to view the character
- Pressing "Play" triggers a smooth Cinemachine camera transition into the gameplay view

## Player Movement
- Movement is controlled via an on screen joystick
- Rigidbody based movement with smooth rotation towards the movement direction
- The UI layout is designed to minimise hand movement and hand stretching

## Dash System
- The player can dash in a direction using:
    - A swipe gesture
    - A shake / flick of the device
- Dash direction is relative to the camera orientation
- The dash system uses:
    - Limited charges
    - Recharge time
    - Cooldown modifiers that can be upgraded

## Camera Controlls
- Gyroscope controlled camera rotation on mobile platforms
- Pinch to zoom allows the player to adjust camera distance which is mobile only
- On Web builds, vibrations, gyro and touch features are automatically disabled

### Controls and Interactions ^


### Combat System v

## Automatic Shooting
- The player automatically fires projectiles at nearby enemies
- Projectile count can be increased through upgrades

## Projectiles
- Projectiles are managed using a pooled system for performance
- No individual projectile scripts are used
- Critical hits Damage scaling via upgrades
- Critical Chance scaling via upgrades
- Projectil count scaling via upgrades

## Enemies and Difficulty Scaling
- Enemies are pooled and use separation behaviour to avoid clumping
- Enemy properties scale over time:
    - Spawn rate
    - Maximum number of enemies
    - Health
    - Movement speed
    - Contact damage
- This creates increasing challenge while encouraging strategic upgrade choices

### Combat System ^


### Chests and Upgrades v

## Chest Interaction
- Chests spawn around the player
- A UI arrow always points towards the nearest chest
- On mobile, chests can be opened by:
    - Swiping upwards
    - Shaking the device upwards

## Upgrade Selection
- Each chest presents three upgrade choices
- Each upgrade has an associated rarity tier
- One reroll is available per chest interaction with an AD

## Upgrade Types
- Max Health
- Heal
- Fire Rate
- Move Speed
- Dash Cooldown
- Dash Recharge
- Additional
- Projectiles
- Critical Chance
- Critical Damage

## Rarity Levels
- Common
- Uncommon
- Rare
- Epic
- Legendary
- Rarity directly scales the strenght of each upgrade

### Chests and Upgrades ^


## User Interface and Feedack
- The game displays clear numerical feedback for all player upgrades
- The player can always see:
    - Health
    - Movement speed multiplier
    - Fire rate multiplier
    - Dash cooldown
    - Dash charges
    - Dash recharge time
    - Projectile count
    - Critical chance
    - Critical damage
- UI placement prioritises accessibility and readability


## Progression and Data Persistance
- The game tracks and saves:
    - Best survival time
    - Highest kill count
    - Total number of attempts
    - Music Volume
- All data is stored using PlayerPrefs and persists between sessions


### Audio and Haptics v

## Audio Settings
- A master volume slider is available in the menu
- Volume settings are saved and restored automatically

## Hactic Feedback for Mobile only
- Different vibration strengths are used for:
    - Dash activation
    - Chest interaction
    - Player death
- Haptics are disabled on unsupported platforms

### Audio and Haptics ^

## Advertisement
- The game includes three advertisement types:
    - Banner advertisement displayed during gameplay
    - Interstitial advertisement shown after player death
    - Rewarded advertisement allowing chest rerolls
- These are integrated in a way that does not interrupt core gameplay flow

### Audio and Haptics ^


## Technical Implementation
- Unity Input System
- Cinemachine camera system
- Object pooling for enemies and projectiles
- Mobile sensor input such as gyroscope and accelerometer
- Platform specific feature handling
- Centralised game state management


## Build Targets
- WebGL for browser deployment
- Android (APK)
- iOS (Xcode project)

## Links 
https://www.youtube.com/watch?v=KLorLToQ3mY&t=273s
https://www.youtube.com/watch?v=6jQIeZk72cA
https://www.youtube.com/watch?v=c9IT_SUjT9M&t=62s
https://www.youtube.com/watch?v=n1WpVJePmnM&t=98s
https://www.youtube.com/watch?v=2kFGmuPHiA0
https://www.youtube.com/watch?v=wpSm2O2LIRM
https://www.youtube.com/watch?v=NqrJHj9xlqY
https://www.youtube.com/watch?v=U08ScgT3RVM
https://www.youtube.com/watch?v=SSHdoGSLsOA&t=141s
https://www.youtube.com/watch?v=p47_LeMEFlY&t=146s
https://www.youtube.com/watch?v=gnxnmw5ryhg
https://discussions.unity.com/t/how-to-optimize-performance-with-many-many-enemies/455811/9
https://gameprogrammingpatterns.com/spatial-partition.html
https://www.gamedev.net/forums/topic/711411-spatial-partitioning-grid-am-i-doing-it-right/
https://www.youtube.com/watch?v=2OLLxUYTC_E
https://www.youtube.com/watch?v=vxZx_PXo-yo
https://www.youtube.com/watch?v=rvl9LCaFHtQ
https://www.youtube.com/watch?v=hP4Vu6JbzSo
https://discussions.unity.com/t/how-to-optimize-performance-with-many-many-enemies/455811/7
https://www.gamedev.net/forums/topic/711411-spatial-partitioning-grid-am-i-doing-it-right/
https://www.youtube.com/watch?v=gnxnmw5ryhg
https://www.youtube.com/watch?v=hP4Vu6JbzSo
https://www.youtube.com/watch?v=2OLLxUYTC_E
https://gameprogrammingpatterns.com/spatial-partition.html
https://learn.unity.com/tutorial/introduction-to-object-pooling
https://medium.com/%40dbmsidley/understanding-object-pooling-in-unity-c-a-performance-optimization-guide-a0f7fa1334c3
https://discussions.unity.com/t/how-to-object-pool-multiple-types/916462
https://www.youtube.com/watch?v=fsEkZLBeTJ8
https://www.youtube.com/watch?v=wpSm2O2LIRM&list=PLc2ULMRcH3rDevKde69Nm59HYaF_RxdBF
https://www.youtube.com/watch?v=dO_YrCV7uk8
https://www.youtube.com/watch?v=_YTC8m9pHyI