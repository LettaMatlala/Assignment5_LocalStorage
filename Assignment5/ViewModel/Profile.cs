using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment5.ViewModel
{
    internal class Profile
    {

        // Represents the user profile data structure
       
            public string Name { get; set; }          // First name
            public string Surname { get; set; }       // Last name
            public string Email { get; set; }         // Email address
            public string Bio { get; set; }           // Short biography
            public string ProfileImagePath { get; set; } // Path to profile picture (optional)
        }
    
}
