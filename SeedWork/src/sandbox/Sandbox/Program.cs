using Cranks.SeedWork.Sandbox;

var v1 = Gender.Female;
Console.WriteLine(v1);

var v2 = v1 with
         {
             Key = "asda",
         };
