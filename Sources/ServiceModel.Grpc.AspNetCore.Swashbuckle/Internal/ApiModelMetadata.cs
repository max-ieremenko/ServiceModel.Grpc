// <copyright>
// Copyright 2021 Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ServiceModel.Grpc.AspNetCore.Swashbuckle.Internal
{
    internal sealed class ApiModelMetadata : ModelMetadata
    {
        public ApiModelMetadata(ModelMetadataIdentity identity)
            : base(identity)
        {
        }

        public override IReadOnlyDictionary<object, object> AdditionalValues { get; } = null!;

        public override string BinderModelName { get; } = null!;

        public override Type BinderType { get; } = null!;

        public override BindingSource BindingSource { get; } = null!;

        public override bool ConvertEmptyStringToNull { get; }

        public override string DataTypeName { get; } = null!;

        public override string Description { get; } = null!;

        public override string DisplayFormatString { get; } = null!;

        public override string DisplayName { get; } = null!;

        public override string EditFormatString { get; } = null!;

        public override ModelMetadata ElementMetadata { get; } = null!;

        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues { get; } = null!;

        public override IReadOnlyDictionary<string, string> EnumNamesAndValues { get; } = null!;

        public override bool HasNonDefaultEditFormat { get; }

        public override bool HideSurroundingHtml { get; }

        public override bool HtmlEncode { get; }

        public override bool IsBindingAllowed => true;

        public override bool IsBindingRequired { get; }

        public override bool IsEnum { get; }

        public override bool IsFlagsEnum { get; }

        public override bool IsReadOnly { get; }

        public override bool IsRequired { get; }

        public override ModelBindingMessageProvider ModelBindingMessageProvider { get; } = null!;

        public override string NullDisplayText { get; } = null!;

        public override int Order { get; }

        public override string Placeholder { get; } = null!;

        public override ModelPropertyCollection Properties { get; } = null!;

        public override IPropertyFilterProvider PropertyFilterProvider { get; } = null!;

        public override Func<object, object> PropertyGetter { get; } = null!;

        public override Action<object, object> PropertySetter { get; } = null!;

        public override bool ShowForDisplay { get; }

        public override bool ShowForEdit { get; }

        public override string SimpleDisplayProperty { get; } = null!;

        public override string TemplateHint { get; } = null!;

        public override bool ValidateChildren { get; }

        public override IReadOnlyList<object> ValidatorMetadata { get; } = null!;

        internal (Type Type, string Name)[]? Headers { get; set; }

        public static ApiModelMetadata ForType(Type type)
        {
            return new ApiModelMetadata(ModelMetadataIdentity.ForType(type));
        }

        public static ApiModelMetadata ForParameter(ParameterInfo parameter)
        {
            return new ApiModelMetadata(ModelMetadataIdentity.ForParameter(parameter));
        }

        public static ApiModelMetadata ForParameter(ParameterInfo parameter, Type modelType)
        {
            return new ApiModelMetadata(ModelMetadataIdentity.ForParameter(parameter, modelType));
        }
    }
}
