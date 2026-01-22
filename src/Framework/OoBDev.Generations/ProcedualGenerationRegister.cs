using OoBDev.Generations.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OoBDev.Generations
{
    public static class ProcedualGenerationRegister
    {
        public static IReadOnlyList<(Type type, Attribute attribute)> ComponentModelExtensions = new List<(Type, Attribute)>
        {
            (typeof(object), new GenerateObjectAttribute()),
            (typeof(object), new GenerateInterfaceAttribute()),
            //(typeof(object), new GenerateAbstractAttribute()), //TODO: this is incomplete so do not register at this time.
            (typeof(object), new GenerateCollectionAttribute()),
            (typeof(object), new GenerateQueryableAttribute()),

            (typeof(Enum), new GenerateEnumerationAttribute()),

            (typeof(bool), new GenerateBooleanAttribute()),
            (typeof(bool?), new GenerateBooleanAttribute()),

            (typeof(object), new GenerateArrayAttribute()),

            (typeof(byte), new GenerateIntegerAttribute()),
            (typeof(byte?), new GenerateIntegerAttribute()),
            (typeof(sbyte), new GenerateIntegerAttribute()),
            (typeof(sbyte?), new GenerateIntegerAttribute()),

            (typeof(short), new GenerateIntegerAttribute()),
            (typeof(short?), new GenerateIntegerAttribute()),
            (typeof(ushort), new GenerateIntegerAttribute()),
            (typeof(ushort?), new GenerateIntegerAttribute()),

            (typeof(int), new GenerateIntegerAttribute()),
            (typeof(int?), new GenerateIntegerAttribute()),
            (typeof(uint), new GenerateIntegerAttribute()),
            (typeof(uint?), new GenerateIntegerAttribute()),

            (typeof(long), new GenerateLongAttribute()),
            (typeof(long?), new GenerateLongAttribute()),
            (typeof(ulong), new GenerateLongAttribute()),
            (typeof(ulong?), new GenerateLongAttribute()),

            (typeof(DateTime), new GenerateDateTimeAttribute()),
            (typeof(DateTime?), new GenerateDateTimeAttribute()),
            (typeof(TimeSpan), new GenerateDateTimeAttribute()),
            (typeof(TimeSpan?), new GenerateDateTimeAttribute()),
            (typeof(DateTimeOffset), new GenerateDateTimeAttribute()),
            (typeof(DateTimeOffset?), new GenerateDateTimeAttribute()),

            (typeof(decimal), new GenerateDoubleAttribute()),
            (typeof(decimal?), new GenerateDoubleAttribute()),
            (typeof(double), new GenerateDoubleAttribute()),
            (typeof(double?), new GenerateDoubleAttribute()),
            (typeof(float), new GenerateDoubleAttribute()),
            (typeof(float?), new GenerateDoubleAttribute()),

            (typeof(Guid), new GenerateGuidAttribute()),
            (typeof(Guid?), new GenerateGuidAttribute()),

            (typeof(string), new GenerateStringAttribute()),

            //TODO: abstract support
            //TODO: collection, list support
            //TODO: dictionary support
            //TODO: iqueryable support

        }.AsReadOnly();

        public static void EnsureExtension()
        {
            var grouped = from ext in ComponentModelExtensions
                          group ext.attribute by ext.type;

            foreach (var ext in grouped)
            {
                TypeDescriptor.AddAttributes(ext.Key, ext.ToArray());
                TypeDescriptor.Refresh(ext.Key);

                //TODO: add the ability to remove/reload extension?
                //var existing = TypeDescriptor.GetProvider(ext.Key);
                //existing.
                ////TODO: check if extension already exists
                //var extendedTypeProvider = new ProcedualGenerationTypeProvider();
                //TypeDescriptor.AddProvider(extendedTypeProvider, ext.Key);

                //var desc = existing.GetTypeDescriptor(ext.Key);
                //extendedTypeProvider.GetTypeDescriptor()
                //TypeDescriptor.RemoveProvider()
                //existing.Attributes.Matches()
                //TypeDescriptor.GetDefaultProperty(ext.Key).Attributes
                //ext.Key
            }
        }

        //TODO: add the ability to remove extension?
        //public static void RemoveExtension()
        //{
        //    //foreach (var ext in ComponentModelExtensions)
        //    //{
        //    //    TypeDescriptor.GetProvider(ext.type).att
        //    //}
        //    //TypeDescriptor.AddAttributes(ext.type, ext.attribute);
        //    //  TypeDescriptor.Refresh()
        //}
    }
}
