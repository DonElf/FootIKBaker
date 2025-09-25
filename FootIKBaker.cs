using UnityEditor;
using UnityEngine;

public class FootIKBaker : EditorWindow {
    // Should I use flags..?
    // Nah. I don't want anybody to select none.
    private enum WeightMode {
        Height,
        Velocity,
        HeightAndVelocity
    }

    // Object references
    private Animator animator;

    // Paramaters
    private float sampleRate = 30f;
    private WeightMode mode = WeightMode.Height;
    private Vector2 heightRange = new(0.1f, 0.15f);
    private Vector2 velocityRange = new(0.05f, 0.25f);

    // The clips are down here because I want them to be. Looks nice.
    [SerializeField] private AnimationClip[] clips; // Has to be serialised in order to display.

    // Just make a window. You've seen this a thousand times before.
    // Also, using IMGUI instead of UI Toolkit. Teehee.
    [MenuItem("Tools/Foot IK Baker")]
    private static void ShowWindow() {
        GetWindow<FootIKBaker>("Foot IK Baker");
    }

    private void OnGUI() {
        // Animator reference
        animator = (Animator)EditorGUILayout.ObjectField("Animator (Humanoid)", animator, typeof(Animator), true);

        // Configuration
        sampleRate = EditorGUILayout.FloatField("Sample Rate", sampleRate);
        mode = (WeightMode)EditorGUILayout.EnumPopup("Weight Mode", mode);
        heightRange = EditorGUILayout.Vector2Field("Height Range", heightRange);
        velocityRange = EditorGUILayout.Vector2Field("Velocity Range", velocityRange);

        // Animation clips
        SerializedObject so = new SerializedObject(this);
        SerializedProperty arrayProp = so.FindProperty("clips");
        EditorGUILayout.PropertyField(arrayProp, true);
        so.ApplyModifiedProperties();

        // The button to bake the weight curves
        if (GUILayout.Button("Bake")) {
            // Null check for the animator & clips.
            if (animator == null || clips == null || clips.Length == 0) {
                Debug.LogError("Assign animator and clips.");
                return;
            }

            // Bake each clip. Another null check, just in case.
            foreach (AnimationClip clip in clips) {
                if (clip != null)
                    BakeCurves(clip);
            }

            // Save immediately. We mark each curve as dirty, so it'll save when you save the project.
            // Keep in mind saving doesn't work great for read only animations... like, in .fbx files and allat. 
            // I would suggest using writable animations (just duplicate the animations in the FBX files).
            //AssetDatabase.SaveAssets();

            // Success!
            Debug.Log(":) Curves baked and saved.");
        }

        // The button to delete the weight curves
        // "Clear" sounds friendlier than "Delete", although it's technically incorrect...
        if (GUILayout.Button("Clear")) {
            // Null check for the clips.
            if (clips == null || clips.Length == 0) {
                Debug.LogError("Assign clips to clear.");
                return;
            }

            // Clear each clip.
            foreach (AnimationClip clip in clips)
                ClearClipIK(clip);

            // Clips are marked dirty. We don't need to save immediately.
            //AssetDatabase.SaveAssets();

            // Success!
            Debug.Log(":) Curves cleared.");
        }
    }

    // The bake button calls this function for every clip.
    private void BakeCurves(AnimationClip clip) {
        // Create new curves, to use as the left and right foot weight curves.
        AnimationCurve leftCurve = new();
        AnimationCurve rightCurve = new();

        // Get the transforms of the feet.
        Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        // Break the clip into "steps", according to the sample rate.
        int steps = Mathf.CeilToInt(clip.length * sampleRate);

        // Used for calculating velocities.
        Vector3 prevLeft = Vector3.zero;
        Vector3 prevRight = Vector3.zero;

        // This doesn't change... Calculate it now.
        float deltaTime = 1f / sampleRate;

        // For each "step" (key) in the clip
        for (int i = 0; i <= steps; i++) {
            // Calculate the time, according to the step & sample rate.
            float t = i / sampleRate;

            // For any clips that are not a clean multiple of the sample rate, the last step will be beyond the end of the animation.
            // So, we just set it to the end.
            if (i == steps)
                t = clip.length;

            // Sample the animation using the animator.
            // This sets the animators position, which we can then get the transforms from.
            clip.SampleAnimation(animator.gameObject, t);

            // Variables to store the weight in.
            float leftWeight = 1;
            float rightWeight = 1;

            // If we're using velocity.
            if (mode != WeightMode.Height) {
                // We can't use velocity on the the first frame, since we need to sample multiple positions.
                // Hence, we continue. To avoid writing a key.
                if (i == 0)
                    continue;

                // Velocity is  in m/s.
                float leftVel = (leftFoot.position - prevLeft).magnitude / deltaTime;
                float rightVel = (rightFoot.position - prevRight).magnitude / deltaTime;

                // Use our velocity to determine our weight.
                // We can just assign, since there's nothing before that could change the weight.
                leftWeight = 1f - Mathf.Clamp01(Mathf.InverseLerp(velocityRange.x, velocityRange.y, leftVel));
                rightWeight = 1f - Mathf.Clamp01(Mathf.InverseLerp(velocityRange.x, velocityRange.y, rightVel));

                // Set the previous positions to the new positions, for the next loop.
                prevLeft = leftFoot.position;
                prevRight = rightFoot.position;
            }

            // If we're using weight.   
            if (mode != WeightMode.Velocity) {
                // Get the Y of the animator/root.
                float animY = animator.transform.position.y;

                // Get the positions of the feet relative to the animator/root, hence ignoring root motion.
                float leftY = leftFoot.position.y - animY;
                float rightY = rightFoot.position.y - animY;

                // We can multiply. That works if it's weighted with velocity, or if it's 1 (default).
                leftWeight *= 1f - Mathf.Clamp01(Mathf.InverseLerp(heightRange.x, heightRange.y, leftY));
                rightWeight *= 1f - Mathf.Clamp01(Mathf.InverseLerp(heightRange.x, heightRange.y, rightY));
            }

            // Now, write the keys to the curves.
            leftCurve.AddKey(t, leftWeight);
            rightCurve.AddKey(t, rightWeight);
        }

        // Write the curves to the animations.
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("", typeof(Animator), "FootIKWeight_L"),
            leftCurve
        );
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("", typeof(Animator), "FootIKWeight_R"),
            rightCurve
        );

        // Mark as dirty, so it saves.
        EditorUtility.SetDirty(clip);
    }

    // Removes the curves.
    private void ClearClipIK(AnimationClip clip) {
        // Write null to both of the curves.
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("", typeof(Animator), "FootIKWeight_L"),
            null
        );
        AnimationUtility.SetEditorCurve(
            clip,
            EditorCurveBinding.FloatCurve("", typeof(Animator), "FootIKWeight_R"),
            null
        );

        // Mark dirty.
        EditorUtility.SetDirty(clip);
    }
}