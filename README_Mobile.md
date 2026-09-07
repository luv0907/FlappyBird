# Mobile build & publish guide

This document lists the minimal steps to build and publish the Unity project to Android and iOS.

Prerequisites
- Install the Android and/or iOS Build Support modules via Unity Hub.
- For Android: install Android SDK/NDK & Java (Unity Hub can install these for you).
- For iOS: macOS with Xcode and an Apple Developer account.

Quick steps — Android (APK/AAB)
1. Open Unity → File → Build Settings → Switch Platform → Android.
2. Player Settings:
   - Set `Bundle Identifier` (e.g. com.yourcompany.firstgame).
   - Set `Version` and `Bundle Version Code`.
   - Configure graphics, orientation, and other settings.
3. Keystore (for publishing):
   - Edit → Project Settings → Player → Publishing Settings → Keystore Manager.
   - Create or assign a keystore and key (store password, key alias/password required for publishing).
4. Use the menu: `Build → Mobile → Build Android APK` or `Build Android AAB`.
5. Test the generated `Builds/Android/game.apk` on a device or upload the `.aab` to Google Play Console.

Quick steps — iOS (Xcode project)
1. Open Unity → File → Build Settings → Switch Platform → iOS.
2. Player Settings:
   - Set `Bundle Identifier` and `Version`.
   - Configure provisioning profile settings (managed in Xcode for many cases).
3. Use the menu: `Build → Mobile → Build iOS Xcode Project`.
4. In Xcode, open the generated project in `Builds/iOS`, set your Team and provisioning, then Archive → Upload to App Store Connect.

Online features
- For leaderboards/auth/cloud saves, use PlayFab, Firebase, or a custom REST API. Do not embed secret keys in the client — use server-side endpoints or cloud functions.

CI / Automation suggestions
- Use Unity Builder GitHub Action or Unity’s Cloud Build to automate builds.
- Add a GitHub Action to run the Unity build and upload artifacts for QA.

If you want, I can:
- Add a GitHub Action that builds WebGL or Android and uploads the artifact.
- Add PlayFab or Firebase integration for leaderboards.
- Walk through keystore provisioning and App Store Connect steps interactively.

Local CLI build (Unity Editor headless)

You can run Unity in batchmode to call the editor build methods added in `Assets/Editor/BuildMobile.cs`.

Example (macOS) — replace the Unity path and method as needed:
```bash
# Build APK (calls BuildMobile.BuildAndroidAPK)
"/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity" -quit -batchmode -projectPath "$(pwd)" -executeMethod BuildMobile.BuildAndroidAPK -logFile Builds/Android/unity_build.log

# Or use the provided helper script:
./scripts/build_android.sh "/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity" BuildMobile.BuildAndroidAAB
```

GitHub Actions notes
- The workflow added at `.github/workflows/android-build.yml` uses `game-ci/unity-builder`. You must set the correct `UNITY_VERSION` in the workflow and configure Unity activation secrets (or use a self-hosted runner with Unity installed).

Secrets you may need to provide in the repo settings:
- `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_SERIAL` — for automated Unity activation (or use a license file approach with `UNITY_LICENSE` secret).

