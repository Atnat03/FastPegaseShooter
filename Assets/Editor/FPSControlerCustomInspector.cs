using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FPSController))]
public class FPSControlerCustomInspector : Editor
{
    // Foldout states
    private bool showParameters = true;
    private bool showUnlockedCapacities = true;
    private bool showCamera = true;
    private bool showMovement = true;
    private bool showHeadbob = true;
    private bool showJump = true;
    private bool showSuperJump = true;
    private bool showWallRide = true;
    private bool showCrouch = true;
    private bool showSlide = true;
    private bool showDash = true;
    private bool showSlopeSlide = true;
    private bool showGapple = true;

    private GUIStyle foldoutStyle;

    private void OnEnable()
    {
        foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSection("References", Color.white, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rb"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraParentTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraSpringTarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_camera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerFeet"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerLeftSide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerRightSide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerInput"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerVisual"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_playerAnimation"));
        }, ref showParameters);

        DrawSection("Parameters", new Color(0.8f, 0.9f, 1f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("landSnap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashVerticality"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clampedMaxAirSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpSlideOnEndOfSlide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("singleClicGrapple"));
        }, ref showParameters);

        DrawSection("Unlocked Capacities", new Color(1f, 0.85f, 0.6f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRideUnlocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideUnlocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashUnlocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("superJumpUnlocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeSlideUnlocked"));
        }, ref showUnlockedCapacities);

        DrawSection("Camera", Color.aliceBlue, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraSpringHalfLife"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraSpringFrequency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rollSmoothing"));
        }, ref showCamera);

        DrawSection("Movement", new Color(0.6f, 1f, 0.6f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mouseSensitivity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("verticalLimit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundMomentumFactor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sideStepImpulseForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallDetectionRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkableSlopeAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStepHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityBonusForceAscending"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityBonusForceFalling"));
        }, ref showMovement);

        DrawSection("Headbob", new Color(1f, 0.6f, 1f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkingHeadbobAmplitude"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkingHeadbobFrequency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRidingHeadbobAmplitude"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRidingHeadbobFrequency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headbobStopReturningSpeed"));
        }, ref showHeadbob);

        DrawSection("Jump", new Color(0.6f, 0.8f, 1f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airControlForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAirSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airDrag"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bufferJumpTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTimeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("landSnapVelocity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airJumpCount"));
        }, ref showJump);

        DrawSection("Super Jump", Color.brown, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("superJumpInputMaxDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("superJumpVerticalForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("superJumpHorizontalForce"));
        }, ref showSuperJump);

        DrawSection("Wall Ride", new Color(1f, 0.6f, 0.6f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRideDetectionRange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRidingDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRideCooldownChangeSide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRideCooldownSameSide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallRidingSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minSpeedToWallRide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpVerticalForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpHorizontalForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headtiltIntensity"));
        }, ref showWallRide);

        DrawSection("Crouch", new Color(0.8f, 1f, 0.8f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraOffsetWhenCrouching"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyStandUpCollider"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topHeightStandUpCollider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyCrouchedCollider"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("topHeightCrouchedCollider"));
        }, ref showCrouch);

        DrawSection("Slide", new Color(1f, 1f, 0.6f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideMinTimeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideMaxTimeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideJumpVerticalForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideJumpHorizontalForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slideCooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteSlideDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraSlideFOV"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slidingBackToNormalSpeedDelay"));
        }, ref showSlide);

        DrawSection("Dash", new Color(0.6f, 1f, 1f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashTimeDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashEnergyCost"));
        }, ref showDash);

        DrawSection("Slope Slide", new Color(1f, 0.75f, 0.4f), () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minSlopeAngleToSlopeSlide"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeSlideMaxSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeInfluenceOnRotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeInfluenceOnVelocity"));
        }, ref showSlopeSlide);

        DrawSection("Grappling", Color.aliceBlue, () =>
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_castWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_castMaxDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_grapplingSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_grappleRedirectionSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_endGrappleImpulseForce"));
        }, ref showGapple);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string title, Color color, System.Action drawContent, ref bool foldoutState)
    {
        EditorGUILayout.Space(8);

        Color previousColor = GUI.color;
        GUI.color = color;

        foldoutState = EditorGUILayout.Foldout(foldoutState, title, true, foldoutStyle);

        GUI.color = previousColor;

        if (foldoutState)
        {
            EditorGUILayout.Space(3);
            drawContent.Invoke();
        }
    }
}