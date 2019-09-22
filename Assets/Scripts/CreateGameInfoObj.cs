using System;
public class CreateGameInfoObj
    {
        public GameInfo ConvertJSON(string JSON)
        {
            return GameInfo.CreateFromJSON(JSON);
        }
    }

