# Foot IK Curve Baker
A small tool for baking foot IK curves, because I didn't want to do it by hand.

Default settings are what I was using for some Mixamo animations... Try those first? :)  
And, as always, you're gonna get better results doing it by hand. But, this method is fast...

## Table of Contents 
* [Comparisons](#comparisons)  
* [Usage Guide](#usage-guide)  
* [Licence](#licence)  

<br> <!-- Using HTML in MD... I know it's bad, but womp womp. -->

## Comparisons
### Before:
![RampNoIK](https://github.com/user-attachments/assets/8fe3d999-e7e6-4d3e-a2c8-797198c056c7)

### After:
![RampIK](https://github.com/user-attachments/assets/2fe39e78-743a-4fea-946e-4caf1cc93262)

<br>

### Before:
![StepsNoIK](https://github.com/user-attachments/assets/0cbe09e0-e33f-468b-9eda-4d5e3bb4dae2)

### After:
![StepsIK](https://github.com/user-attachments/assets/0440a599-deef-4e3d-881b-a7d1d17f695f)

<br>

## Usage Guide
To use, add the FootIKBaker.cs to your project (Preferable in an Editor folder).  
Then, you should be able to open the Foot IK Baker window, located at `Tools` -> `Foot IK Baker`.  
<img width="220" height="107" alt="image" src="https://github.com/user-attachments/assets/3d4a5f84-4376-48c1-a548-575d2425ae93" />  
<img width="374" height="453" alt="image" src="https://github.com/user-attachments/assets/abdde0ff-7f09-43b6-8aaf-3a326d8c5f14" />  

Next, add your animator and clips. The animator should be a reference to a humanoid animator component.  
<img width="358" height="440" alt="image" src="https://github.com/user-attachments/assets/4e7c765d-909f-4b78-bc45-49322701185d" />  

Then, press bake. You should now be able to see the keyframes in your animation.  
<img width="1220" height="317" alt="image" src="https://github.com/user-attachments/assets/6923e774-a05f-40dc-923b-a2d40a5275a2" />  

To access these values from a script, add the float paramaters `FootIKWeight_L` and `FootIKWeight_R` to your animation controller.  
<img width="390" height="120" alt="image" src="https://github.com/user-attachments/assets/1bd7efc9-c7f4-4d61-821f-9f2506ce953f" />  

You can now get the values using a reference to the animator, with `animator.GetFloat("FootIKWeight_L")` and `animator.GetFloat("FootIKWeight_R")`.

<br>

## Licence

   Copyright 2025 Don_Elf

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

   [`http://www.apache.org/licenses/LICENSE-2.0`](http://www.apache.org/licenses/LICENSE-2.0)

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
