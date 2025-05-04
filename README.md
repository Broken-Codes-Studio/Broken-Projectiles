<p align="center">
  <a href="https://godotengine.org/download/windows/">
      <img alt="Static Badge" src="https://img.shields.io/badge/Godot-4.4%2B-blue">
  </a>
  <a href="LICENSE">
    <img alt="GitHub License" src="https://img.shields.io/github/license/Broken-Codes-Studio/BrokenSigil">
  </a>
</p>

# What is it?
**Broken Projectiles** is a Godot C# add-on that provides a collection of scripts and pre-built scenes for handling various types of projectiles. From basic projectiles to beams and grenades, this add-on is designed to be modular, flexible, and easy to integrate into any project.

<hr>

# Features
- **Modular Design:** Implement and manage projectiles, beams, and grenades with a flexible framework.
    
- **Extensibility:** Easily create custom mechanics by inheriting from the base system.
    
- **Godot 4.4 Compatibility:** Designed specifically for Godot 4.4, leveraging its C# scripting capabilities.
    
- **Easy Integration:** Simply install and configure within your Godot project.

## Installation

**Note:**  
This addon depends on the **[BrokenSigil](https://github.com/Broken-Codes-Studio/BrokenSigil)** addon. Please ensure it is installed and properly configured in your project.

### Github Main (Latest - Unstable)
1. Install the `BrokenSigil` addon
2. Download the latest `main branch`.
3. Extract the zip file and move the `BrokenProjectiles` folder into project's `addons` folder.
4. Enable the plugin inside `Project/Project Settings/Plugins`.

## How To Use
- To create a custom **Projectile**, **Beam**, or **Grenade**, start by inheriting from the default scenes found in the `addon/BrokenProjectiles/Prefabs` folder.

- Each script emits a **signal** when a hit is detected, passing the hit **collider** as a parameter. You can connect this signal to a custom script to handle specific behavior.
	For example:
	1. Create a custom node like `DamageNode`.
	2. Connect the projectile’s hit signal to a function like `damage(Node3D/2D collider)` in `DamageNode`.
	3. This allows you to apply damage or other effects to any collider the projectile interacts with.

- For more advanced or complex functionality, you can directly **inherit from the base Projectile, Beam, or Grenade scripts** and extend them by creating your own custom nodes and logic.

<hr>

## TODO
- Implement a **Cluster Creation** feature (spawn multiple projectiles or grenades upon bullet impact or grenade explosion).
    
- Add **Target Position Prediction** to **TrackingProjectile** for better accuracy when tracking moving targets
    
- Add **Beam** with **area effect**.
    
- Add a **Bouncy Beam** that can reflect off surfaces.
    
- Create **2D variants** of all projectiles, beams, and grenades.

For more help,
see [Godot's official documentation](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/installing_plugins.html)