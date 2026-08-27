using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace TrackoApi.Models.Base
{
    /// The Rule type
    [Table("mRule")]
    public class Rule:AuditableEntity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        [MaxLength(150)]
        public string RuleKey { get; set; }
        public string Description { get; set; }
        public RuleNature RuleNature { get; set; } = RuleNature.Validation;
        public string ValidationDefination { get; set; }
        public string AssignmentDefination { get; set; }
        public string FailedMessage { get; set; }
        public string SuccessMessage { get; set; }
        public bool TerminateOnError { get; set; }
        public bool ReturnOnSuccess { get; set; }
        public bool IsActive { get; set; }
        public string AppId { get; set; }
        public int ExecutionOrder { get; set; }
        
    }

    public class CompiledRule<T> where T:class
    {
        public Func<T, bool> IsValid { get; set; }
        public Delegate ApplyLogic { get; set; }
        public Rule Rule { get; set; }
    }
    public enum RuleNature
    {
        Validation=0,
        Assignment=1
    }
    /// Author: Cole Francis, Architect
    /// The pre-compiled rules type
    /// https://www.psclistens.com/insight/blog/quickly-build-a-business-rules-engine-using-c-and-lambda-expression-trees/
    //public class PrecompiledRules
    //{
        /////
        ///// A method used to precompile rules for a provided type
        ///// 
        //public static List<Func<T, bool>> CompileRule<T>(List<Rule> rules)
        //{
        //    var compiledRules = new List<Func<T, bool>>();

        //    // Loop through the rules and compile them against the properties of the supplied shallow object 
        //    rules.ForEach(rule =>
        //    {
        //        var genericType = Expression.Parameter(typeof(T));
        //        var key = Expression.Property(genericType, rule.ComparisonPredicate);
        //        var propertyType = typeof(T).GetProperty(rule.ComparisonPredicate)?.PropertyType;
        //        if (propertyType != null)
        //        {
        //            var value = Expression.Constant(Convert.ChangeType(rule.ComparisonValue, propertyType));
        //            var binaryExpression = Expression.MakeBinary(rule.ComparisonOperator, key, value);

        //            compiledRules.Add(Expression.Lambda<Func<T, bool>>(binaryExpression, genericType).Compile());
        //        }
        //    });
        //    // Return the compiled rules to the caller
        //    return compiledRules;
        //}
    //}
    
    //List<Rule> rules = new List<Rule>
    //{
    //// Create some rules using LINQ.ExpressionTypes for the comparison operators
    //new Rule ( "Year", ExpressionType.GreaterThan, "2012"),
    //new Rule ( "Make", ExpressionType.Equal, "El Diablo"),
    //new Rule ( "Model", ExpressionType.Equal, "Torch" )
    //};

    //var compiledMakeModelYearRules = PrecompiledRules.CompileRule(new List<ICar>(), rules);

    // Create a list to house your test cars
    //    List cars = new List();

    //    // Create a car that's year and model fail the rules validations      
    //    Car car1_Bad = new Car
    //    {
    //    Year = 2011,
    //    Make = "El Diablo",
    //    Model = "Torche"
    //    };

    //    // Create a car that meets all the conditions of the rules validations
    //    Car car2_Good = new Car
    //    {
    //    Year = 2015,
    //    Make = "El Diablo",
    //    Model = "Torch"
    //    };

    //    // Add your cars to the list
    //    cars.Add(car1_Bad);
    //    cars.Add(car2_Good);

    //// Iterate through your list of cars to see which ones meet the rules vs. the ones that don't
    //    cars.ForEach(car => {
    //    if (compiledMakeModelYearRules.TakeWhile(rule => rule(car)).Count() > 0)
    //    {
    //    Console.WriteLine(string.Concat("Car model: ", car.Model, " Passed the compiled rules engine check!"));
    //    }
    //    else
    //    {
    //    Console.WriteLine(string.Concat("Car model: ", car.Model, " Failed the compiled rules engine check!"));
    //    }
    //});

    //Console.WriteLine(string.Empty);
    //Console.WriteLine("Press any key to end...");
    //Console.ReadKey();
}
