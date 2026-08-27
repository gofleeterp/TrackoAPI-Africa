using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Unity;

namespace TrackoApi.Unity
{
    public class UnityModelValidator : DataAnnotationsModelValidator
    {
        private readonly IUnityContainer _unityContainer;

        public UnityModelValidator(IEnumerable<ModelValidatorProvider> providers,
            ValidationAttribute attribute)
            : base(providers, attribute)
        {
            this._unityContainer =
                System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver
                    .GetService(typeof(IUnityContainer)) as IUnityContainer;
        }

        public override IEnumerable<ModelValidationResult> Validate(ModelMetadata metadata, object container)
        {
            try
            {
                _unityContainer.BuildUp(Attribute.GetType(), Attribute);
            }
            catch (ResolutionFailedException)
            {
                //Don't understand why it sometimes tries to use Unity to create an attribute rather than just build up an existing object. If this happens it can fail but we want to ignore it.
            }
            string displayName = metadata.GetDisplayName();
            var validations= base.Validate(metadata, container).ToList();
            //System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(container ?? metadata.Model, new UnityServiceLocator(_unityContainer), null)
            //{
            //    DisplayName = displayName,
            //    MemberName = displayName
            //};
            //ValidationResult result = Attribute.GetValidationResult(metadata.Model, validationContext);
            //if (result != ValidationResult.Success)
            //{
            //    validations.Add(new ModelValidationResult
            //    {
            //        Message = result?.ErrorMessage
            //    });
            //}
            return validations;
        }
    }
}
