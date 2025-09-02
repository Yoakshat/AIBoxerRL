public static class Constants {
    public static readonly string[] blockStates = {"CenterBlock", "LeftBlock", "RightBlock"};
    public static readonly string[] attackStates = {"RightHook", "Uppercut", "JabCross", "Knee", 
                              "BodyJab", "Headbutt"};

    public static readonly string[] allMoves = {"RightHook", "Uppercut", "JabCross", "Knee", 
                              "BodyJab", "Headbutt", "CenterBlock", 
                              "LeftBlock", "RightBlock"};

    public static readonly string[] reactionStates = {"SideHit", "RibHit", "HeadSide",
                                                     "HeadBack", "StomachHit", "BodyForward"};

    // blocks take 20 energy (makes sure you can't spam blocks)
    public static readonly float[] energies = {35, 70, 50, 35, 40, 40, 0, 0, 0};

}