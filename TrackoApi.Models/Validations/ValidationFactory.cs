using System;
using FluentValidation;
using FluentValidation.Attributes;
using Unity;

namespace TrackoApi.Models.Validations
{
    public class UnityValidationFactory: IValidatorFactory
    {
        private readonly IUnityContainer _container;
        public UnityValidationFactory()
        {
            _container = null;
        }

        public UnityValidationFactory(IUnityContainer container)
        {
            _container = container;
        }
        public IValidator CreateInstance(Type validatorType)
        {
            return _container.Resolve(validatorType) as IValidator;
        }
        public IValidator<T> GetValidator<T>()
        {
            return (IValidator<T>)this.GetValidator(typeof(T));
        }

        public IValidator GetValidator(Type type)
        {
            if (type == null)
                return null;

            var attribute = (ValidatorAttribute)Attribute.GetCustomAttribute(type, typeof(ValidatorAttribute));

            if (attribute == null || attribute.ValidatorType == null)
                return this.CreateInstance(typeof(IValidator<>).MakeGenericType(type));

            return this.CreateInstance(attribute.ValidatorType) as IValidator;
        }
    }
}
