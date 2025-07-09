# Beergeon_v2

A first-person 3D dungeon crawler featuring billboard-style 2D sprites. Includes procedural dungeon generation, player classes and stats, skills, inventory, loot system, and more.

## Getting Started

Follow these instructions to get a copy of the project running on your local machine.

## Git Workflow Guide

### 1. Cloning the Repository

First, clone the repository to your local machine:

```bash
git clone https://github.com/j3ksa/beergeon_v2.git
cd beergeon_v2
```

### 2. Creating a New Branch

Always create a new branch for your feature or bug fix:

```bash
git checkout -b feature/feature-description
```

Or for bug fixes:

```bash
git checkout -b bug/bug-description
```

### 3. Making Changes

Make your changes to the code, then stage and commit them:

```bash
git add .
git commit -m "Add descriptive commit message"
```

### 4. Pushing Your Branch

Push your branch to the remote repository:

```bash
git push origin your-branch-name
```

### 5. Creating a Pull Request

1. Go to the GitHub repository page
2. Click "Compare & pull request" button
3. Fill out the pull request template
4. Add reviewers if required
5. Click "Create pull request"

### 6. Handling Merge Conflicts

If you encounter merge conflicts during a pull request:

1. Pull the latest changes from master:
```bash
git checkout master
git pull
git checkout your-branch-name
git merge master
```

2. Resolve conflicts in your editor:
   - Look for conflict markers: `<<<<<<<`, `=======`, `>>>>>>>`
   - Edit the files to resolve conflicts
   - Remove conflict markers

3. Add and commit the resolved files:
```bash
git add .
git commit -m "Resolve merge conflicts"
```

4. Push the updated branch:
```bash
git push origin your-branch-name
```

### 7. Updating Master Branch

After your pull request is merged, update your local master:

```bash
git checkout master
git pull
```

## Branch Naming Conventions

- `feature/description` - for new features
- `bug/description` - for bug fixes

## Commit Message Guidelines

- Use present tense ("Add feature" not "Added feature")
- Keep the first line under 50 characters
- Add detailed description if needed after a blank line
- Reference issues and pull requests when applicable

## Folder Structure

Project folders follow Unity best practices. Below is the structure inside the Assets/ directory:

```
Assets/
├── Art/                # 2D sprites, 3D models, textures, materials
│   ├── Characters/
│   ├── Environment/
│   └── UI/
├── Audio/              # Music and sound effects
├── Scenes/             # Unity scene files
├── Scripts/            # C# scripts, organized by feature
│   ├── Player/
│   ├── Enemy/
│   ├── Inventory/
│   └── Systems/
├── Prefabs/            # Reusable GameObjects (enemies, items, etc.)
├── UI/                 # Canvas objects, fonts, HUD prefabs
├── Data/               # ScriptableObjects (items, skills, classes)
├── Plugins/            # Third-party assets
├── Animations/         # Animator Controllers and Animation Clips
└── Docs/               # Game Design Document (GDD), flowcharts, specs
```

💡 Keep all asset names in PascalCase and avoid using spaces. Stick to consistent, descriptive names.

## Documentation

Project documentation is located in:
`/Assets/Docs/`

This folder should includes:
    - Feature specifications
    - Loot tables, skill trees, stat charts
    - Level generation logic

Use Markdown. For collaboration, you can also share links in Discord.

## Tools Used
- [GitHub](https://github.com/j3ksa/beergeon_v2)
    - [Ticketing](https://github.com/j3ksa/beergeon_v2/issues)
    - [Roadmap](https://github.com/j3ksa/beergeon_v2/milestones)

- [Discord](https://discord.gg/5KKZ6tbb)