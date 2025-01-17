//public enum Stage
//{
//    None,

//    Tokenize,

//    AST_parse,

//    DisoverModule_Begin,
//    DisoverModule_Done_LLVMTypes,
//    DisoverModule_Done_RegisterRefs,
//    DisoverModule_Done_ResolveRefs,

//    Generate
//}

//public static class Access
//{
//    public static Stage current;

//    public static void Require(Stage requiredStage)
//    {
//        if (current < requiredStage)
//        {
//            throw new Exception_Access(requiredStage, current);
//        }
//    }
//    public static void Set(Stage newStage)
//    {
//        if (current > newStage)
//        {
//            throw new Exception($"Failed to set new stage ({newStage}) because it has lower level (int value) than current stage ({current})");
//        }
//        current = newStage;
//    }
//}
