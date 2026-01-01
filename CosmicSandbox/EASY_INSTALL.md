# 🚀 Easy Installation Guide for Cosmic Sandbox
## For Complete Beginners - No Technical Knowledge Required!

---

## ⚠️ IMPORTANT: What You Need First

Before starting, your computer MUST have:
- ✅ **Windows 11** (this app won't work on Windows 10, Mac, or Linux)
- ✅ **Internet connection** (to download required software)
- ✅ **At least 5 GB of free disk space**

---

## 📥 Step 1: Get the Project Files

### Option A: If you have the files already
- If someone gave you a USB drive or zip file with the Cosmic Sandbox folder, skip to **Step 2**

### Option B: Download from GitHub
1. Open your web browser (Edge, Chrome, or Firefox)
2. Go to: `https://github.com/MGladstein/theHarvester`
3. Click the green **"Code"** button
4. Click **"Download ZIP"**
5. Once downloaded, **right-click** the ZIP file
6. Choose **"Extract All..."**
7. Click **"Extract"**
8. Remember where you extracted it! (Usually in your Downloads folder)

---

## 🔧 Step 2: Install Required Software

You need to install some free Microsoft software first. Don't worry - it's safe and official!

### A) Install .NET 8.0 SDK

1. **Open your web browser**
2. **Go to**: `https://dotnet.microsoft.com/download/dotnet/8.0`
3. Look for the section that says **"Build apps - SDK"**
4. Click the **Windows x64** installer download button
5. **Wait for the download** to complete (file is about 200 MB)
6. **Find the downloaded file** (usually in your Downloads folder)
   - It's named something like: `dotnet-sdk-8.0.xxx-win-x64.exe`
7. **Double-click** the installer file
8. Click **"Yes"** if Windows asks for permission
9. Click **"Install"** in the installer window
10. **Wait patiently** - this takes 2-5 minutes
11. Click **"Close"** when it's done

### B) Install Visual Studio 2022 Community (FREE)

This is Microsoft's development tool - it's completely free for personal use!

1. **Open your web browser**
2. **Go to**: `https://visualstudio.microsoft.com/downloads/`
3. Under **"Community"**, click **"Free download"**
4. **Wait for the download** (it's a small installer, about 3 MB)
5. **Find the downloaded file**: `VisualStudioSetup.exe`
6. **Double-click** to run it
7. Click **"Yes"** if Windows asks for permission
8. Click **"Continue"** in the installer
9. **IMPORTANT**: When you see checkboxes with different options:
   - ✅ Check the box for **".NET desktop development"**
   - ✅ Check the box for **"Desktop development with C++"** (this ensures DirectX support)
10. Click **"Install"** button (bottom right)
11. **Go get coffee** ☕ - This downloads and installs about 5-10 GB of files
    - Takes 20-60 minutes depending on your internet speed
12. Click **"Close"** when installation is complete
13. **Restart your computer** (important!)

---

## 🎯 Step 3: Build the Cosmic Sandbox App

Now the fun part! We're going to build the app.

### Option A: EASIEST - Use the Automatic Build Script

1. **Open File Explorer** (the folder icon on your taskbar)
2. **Navigate to** where you extracted the files
3. Find the **CosmicSandbox** folder
4. **Double-click** to open it
5. Look for a folder called **"build"**
6. **Double-click** the "build" folder
7. **Right-click** on the file `build.ps1`
8. Choose **"Run with PowerShell"**

   **If you see a security warning:**
   - Right-click `build.ps1` again
   - Choose **"Properties"**
   - At the bottom, check the box: **"Unblock"**
   - Click **"OK"**
   - Try **"Run with PowerShell"** again

9. **A blue window will appear** with scrolling text
10. **Wait** for it to finish (1-5 minutes)
11. When you see **"Build Complete"** in green, it's done!
12. The app is now ready in the **"publish"** folder

### Option B: Use Visual Studio (If automatic script doesn't work)

1. **Open File Explorer**
2. **Navigate to** the CosmicSandbox folder
3. **Double-click** the file: `CosmicSandbox.sln`
   - This will open Visual Studio (might take 30 seconds to load)
4. **Wait** for Visual Studio to fully load
5. At the top of the window, find the dropdown that says **"Debug"**
6. **Click it** and change it to **"Release"**
7. Click the **"Build"** menu at the top
8. Click **"Build Solution"**
9. **Wait** for it to finish (you'll see "Build succeeded" at the bottom)
10. Click **"Build"** menu again
11. Click **"Publish Selection"**
12. Follow the prompts, choosing "Folder" as the target
13. Click **"Publish"**

---

## 🎮 Step 4: Run Cosmic Sandbox!

### Finding and Running the App

1. **Open File Explorer**
2. **Go to** your CosmicSandbox folder
3. **Open** the "publish" folder (or if using Visual Studio, look in "bin\Release\net8.0-windows")
4. **Look for**: `CosmicSandbox.exe` (has a space/planet icon)
5. **Double-click** `CosmicSandbox.exe`

**If Windows shows a security warning:**
- Click **"More info"**
- Click **"Run anyway"**
- This is normal for apps you build yourself!

### 🎉 SUCCESS! The App Should Now Open!

You should see a window with:
- Left panel: Object library (preset scenes)
- Center: Black 3D viewport
- Right panel: Inspector with properties

---

## 🚦 Quick Start - Your First Simulation

1. **In the left panel**, click the button: **"Solar System"**
2. **Press the Play button** ▶ (or press Space on your keyboard)
3. **Watch** the planets orbit!
4. **Use your mouse** in the center viewport:
   - **Left-click and drag** to rotate the camera
   - **Right-click and drag** to pan
   - **Scroll wheel** to zoom in/out
5. **Adjust speed** with the slider at the top (try 10x or 100x!)

---

## ❌ Troubleshooting - If Something Goes Wrong

### "I don't have Windows 11!"
- Sorry! This app ONLY works on Windows 11
- It won't work on Windows 10, Mac, or Linux
- You can check your Windows version: Press **Windows Key + I** → Click **System** → Check under "Windows specifications"

### "The build failed with errors!"
**Try this:**
1. Make sure you installed BOTH .NET SDK and Visual Studio
2. Restart your computer
3. Try the build again

**Still not working?**
- Open Command Prompt (search for "cmd" in Start menu)
- Type: `dotnet --version`
- Press Enter
- You should see: `8.0.xxx`
- If you see an error, .NET SDK didn't install correctly - try installing it again

### "The app won't start!"
1. **Make sure you have Windows 11** (not Windows 10)
2. **Update your graphics drivers**:
   - Right-click the Start button
   - Click "Device Manager"
   - Expand "Display adapters"
   - Right-click your graphics card
   - Click "Update driver"
   - Choose "Search automatically for drivers"

### "I see a blank black screen!"
- This is normal! The 3D rendering is set up but you need to load a scene first
- Click **"Solar System"** in the left panel
- Then click the Play button ▶

### "I get an error about DirectX!"
- Your computer needs to support DirectX 11
- Most computers made after 2012 support this
- Try updating your graphics drivers (see above)

---

## 🆘 Still Need Help?

### Check the Log Files
If the app crashes, it creates log files here:
```
C:\Users\[YourUsername]\AppData\Local\CosmicSandbox\Logs\
```

You can open these with Notepad to see what went wrong.

### Ask for Help
You can report issues, but you'll need to provide:
1. Your Windows version
2. The exact error message (take a screenshot!)
3. What you were doing when the error happened
4. The log file (from the location above)

---

## 🎯 Summary - The Whole Process

```
1. Download/extract the Cosmic Sandbox files
   ↓
2. Install .NET 8.0 SDK (5 minutes)
   ↓
3. Install Visual Studio 2022 Community (30-60 minutes)
   ↓
4. Restart your computer
   ↓
5. Run the build.ps1 script (5 minutes)
   ↓
6. Double-click CosmicSandbox.exe
   ↓
7. Load Solar System preset and press Play!
   ↓
🎉 Enjoy simulating the universe!
```

**Total time needed:** 1-2 hours (most of it is just waiting for downloads)

**Once set up:** You can run CosmicSandbox.exe anytime without repeating these steps!

---

## 💡 Pro Tips

- **Create a Desktop Shortcut**:
  1. Right-click `CosmicSandbox.exe`
  2. Choose "Create shortcut"
  3. Drag the shortcut to your Desktop

- **Bookmark these links** for easy access:
  - .NET SDK: https://dotnet.microsoft.com/download/dotnet/8.0
  - Visual Studio: https://visualstudio.microsoft.com/downloads/

- **Keep the Cosmic Sandbox folder** - don't delete it after building! The app needs these files to run.

---

**Good luck! You've got this!** 🚀🌌
