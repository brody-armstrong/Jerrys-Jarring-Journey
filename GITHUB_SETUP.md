# GitHub Setup Instructions

## 📦 Quick Setup (5 minutes)

### 1. Create GitHub Repository

1. Go to **github.com** and log in
2. Click **"New"** or **"+"** → **"New repository"**
3. Name it: **jerrys-jarring-journey**
4. Description: "2D endless runner skiing game - Unity"
5. **PUBLIC** (so graders can see it)
6. **DON'T** initialize with README (we already have one!)
7. Click **"Create repository"**

---

### 2. Push Your Code to GitHub

Open Terminal and run these commands:

```bash
cd "/Users/brody/Jerry'sJarringJourney"

# Initialize git
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit - Jerry's Jarring Journey complete"

# Add your GitHub repo (replace YOUR_USERNAME)
git remote add origin https://github.com/YOUR_USERNAME/jerrys-jarring-journey.git

# Push to GitHub
git branch -M main
git push -u origin main
```

---

### 3. Verify Upload

1. Refresh your GitHub repo page
2. You should see:
   - ✅ Assets/ folder
   - ✅ README.md
   - ✅ .gitignore
   - ✅ All your scripts
   - ❌ NO Library/ or Temp/ folders (ignored by .gitignore)

---

## ✅ What Gets Uploaded:

- ✅ All scripts (.cs files)
- ✅ All assets (sprites, audio, scenes)
- ✅ Project settings
- ✅ README.md

## ❌ What Gets Ignored (.gitignore):

- ❌ Library/ (Unity cache - huge!)
- ❌ Temp/ (temporary files)
- ❌ Logs/
- ❌ .vs/ (Visual Studio cache)
- ❌ *.csproj, *.sln (auto-generated)

---

## 🎯 Final Checklist:

- [ ] GitHub repo created
- [ ] Code pushed successfully
- [ ] README.md visible on GitHub
- [ ] Assets folder uploaded
- [ ] No Library/Temp folders (should be ignored)
- [ ] Repository is PUBLIC

---

**Done! Share the GitHub link with your instructor!** 🎮

