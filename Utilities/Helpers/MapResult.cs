using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApi.Utilities.Helpers{
    public class MapResultToEnum
    {
        public static string MapResultToString(string input)
        {
            return input switch
            {
                "white" => "white_wins",
                "white_wins" => "white_wins",
                "black" => "black_wins",
                "black_wins" => "black_wins",
                "draw" => "draw",
                "abandoned" => "abandoned",
                _ => "draw"
            };
        }
    }

}