@secure()
param operations_get_updatable_passes_type string

@secure()
param operations_get_updatable_passes_type_1 string

@secure()
param operations_get_updatable_passes_type_2 string

@secure()
param operations_get_updated_badge_type string

@secure()
param operations_get_updated_badge_type_1 string

@secure()
param operations_register_pass_type string

@secure()
param operations_register_pass_type_1 string

@secure()
param operations_register_pass_type_2 string

@secure()
param operations_unregister_type string

@secure()
param operations_unregister_type_1 string

@secure()
param operations_unregister_type_2 string
param service_nohomersapimgmt_name string = 'nohomersapimgmt'

resource service_nohomersapimgmt_name_resource 'Microsoft.ApiManagement/service@2024-06-01-preview' = {
  name: service_nohomersapimgmt_name
  location: 'West Europe'
  sku: {
    name: 'Consumption'
    capacity: 0
  }
  properties: {
    publisherEmail: 'sebastian.gfeller@gmail.com'
    publisherName: 'No Homers Association'
    notificationSenderEmail: 'apimgmt-noreply@mail.windowsazure.com'
    hostnameConfigurations: [
      {
        type: 'Proxy'
        hostName: '${service_nohomersapimgmt_name}.azure-api.net'
        negotiateClientCertificate: false
        defaultSslBinding: true
        certificateSource: 'BuiltIn'
      }
    ]
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Protocols.Server.Http2': 'False'
    }
    virtualNetworkType: 'None'
    disableGateway: false
    natGatewayState: 'Unsupported'
    apiVersionConstraint: {}
    publicNetworkAccess: 'Enabled'
    legacyPortalStatus: 'Disabled'
    developerPortalStatus: 'Enabled'
    releaseChannel: 'Default'
  }
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa 'Microsoft.ApiManagement/service/apis@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_resource
  name: 'no-homers-pass-aswa'
  properties: {
    displayName: 'No Homers Pass ASWA'
    apiRevision: '1'
    subscriptionRequired: false
    serviceUrl: 'https://nice-field-03dde2c03.4.azurestaticapps.net'
    path: 'v1'
    protocols: [
      'https'
    ]
    authenticationSettings: {
      oAuth2AuthenticationSettings: []
      openidAuthenticationSettings: []
    }
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
      query: 'subscription-key'
    }
    isCurrent: true
  }
}

resource service_nohomersapimgmt_name_policy 'Microsoft.ApiManagement/service/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_resource
  name: 'policy'
  properties: {
    value: '<!--\r\n    IMPORTANT:\r\n    - Policy elements can appear only within the <inbound>, <outbound>, <backend> section elements.\r\n    - Only the <forward-request> policy element can appear within the <backend> section element.\r\n    - To apply a policy to the incoming request (before it is forwarded to the backend service), place a corresponding policy element within the <inbound> section element.\r\n    - To apply a policy to the outgoing response (before it is sent back to the caller), place a corresponding policy element within the <outbound> section element.\r\n    - To add a policy position the cursor at the desired insertion point and click on the round button associated with the policy.\r\n    - To remove a policy, delete the corresponding policy statement from the policy document.\r\n    - Policies are applied in the order of their appearance, from the top down.\r\n-->\r\n<policies>\r\n  <inbound></inbound>\r\n  <backend>\r\n    <forward-request />\r\n  </backend>\r\n  <outbound></outbound>\r\n</policies>'
    format: 'xml'
  }
}

resource Microsoft_ApiManagement_service_products_service_nohomersapimgmt_name_no_homers_pass_aswa 'Microsoft.ApiManagement/service/products@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_resource
  name: 'no-homers-pass-aswa'
  properties: {
    displayName: 'No Homers Pass ASWA'
    description: 'Azure static web app that handles Apple Wallets'
    subscriptionRequired: false
    state: 'published'
  }
}

resource service_nohomersapimgmt_name_master 'Microsoft.ApiManagement/service/subscriptions@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_resource
  name: 'master'
  properties: {
    scope: '${service_nohomersapimgmt_name_resource.id}/'
    displayName: 'Built-in all-access subscription'
    state: 'active'
    allowTracing: false
  }
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_get_updatable_passes 'Microsoft.ApiManagement/service/apis/operations@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'get-updatable-passes'
  properties: {
    displayName: 'Get Updatable Passes'
    method: 'GET'
    urlTemplate: '/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}'
    templateParameters: [
      {
        name: 'deviceLibraryIdentifier'
        required: true
        values: []
        type: operations_get_updatable_passes_type
      }
      {
        name: 'passTypeIdentifier'
        required: true
        values: []
        type: operations_get_updatable_passes_type_1
      }
    ]
    request: {
      queryParameters: [
        {
          name: 'passesUpdatedSince'
          values: []
          type: operations_get_updatable_passes_type_2
        }
      ]
      headers: []
      representations: []
    }
    responses: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_get_updated_badge 'Microsoft.ApiManagement/service/apis/operations@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'get-updated-badge'
  properties: {
    displayName: 'Get updated Badge'
    method: 'GET'
    urlTemplate: '/passes/{passTypeIdentifier}/{serialNumber}'
    templateParameters: [
      {
        name: 'passTypeIdentifier'
        required: true
        values: []
        type: operations_get_updated_badge_type
      }
      {
        name: 'serialNumber'
        required: true
        values: []
        type: operations_get_updated_badge_type_1
      }
    ]
    responses: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_log 'Microsoft.ApiManagement/service/apis/operations@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'log'
  properties: {
    displayName: 'Log'
    method: 'POST'
    urlTemplate: '/log'
    templateParameters: []
    responses: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_register_pass 'Microsoft.ApiManagement/service/apis/operations@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'register-pass'
  properties: {
    displayName: 'Register Pass'
    method: 'POST'
    urlTemplate: '/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}'
    templateParameters: [
      {
        name: 'deviceLibraryIdentifier'
        required: true
        values: []
        type: operations_register_pass_type
      }
      {
        name: 'passTypeIdentifier'
        required: true
        values: []
        type: operations_register_pass_type_1
      }
      {
        name: 'serialNumber'
        required: true
        values: []
        type: operations_register_pass_type_2
      }
    ]
    responses: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_unregister 'Microsoft.ApiManagement/service/apis/operations@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'unregister'
  properties: {
    displayName: 'Unregister'
    method: 'DELETE'
    urlTemplate: '/devices/{deviceLibraryIdentifier}/registrations/{passTypeIdentifier}/{serialNumber}'
    templateParameters: [
      {
        name: 'deviceLibraryIdentifier'
        required: true
        values: []
        type: operations_unregister_type
      }
      {
        name: 'passTypeIdentifier'
        required: true
        values: []
        type: operations_unregister_type_1
      }
      {
        name: 'serialNumber'
        required: true
        values: []
        type: operations_unregister_type_2
      }
    ]
    responses: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_default 'Microsoft.ApiManagement/service/apis/wikis@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'default'
  properties: {
    documents: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_no_homers_pass_aswa 'Microsoft.ApiManagement/service/products/apis@2024-06-01-preview' = {
  parent: Microsoft_ApiManagement_service_products_service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'no-homers-pass-aswa'
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_68138646d06bc91fb8db3767 'Microsoft.ApiManagement/service/products/groupLinks@2024-06-01-preview' = {
  parent: Microsoft_ApiManagement_service_products_service_nohomersapimgmt_name_no_homers_pass_aswa
  name: '68138646d06bc91fb8db3767'
  properties: {
    groupId: '${service_nohomersapimgmt_name_resource.id}/groups/administrators'
  }
}

resource Microsoft_ApiManagement_service_products_wikis_service_nohomersapimgmt_name_no_homers_pass_aswa_default 'Microsoft.ApiManagement/service/products/wikis@2024-06-01-preview' = {
  parent: Microsoft_ApiManagement_service_products_service_nohomersapimgmt_name_no_homers_pass_aswa
  name: 'default'
  properties: {
    documents: []
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_get_updatable_passes_policy 'Microsoft.ApiManagement/service/apis/operations/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa_get_updatable_passes
  name: 'policy'
  properties: {
    value: '<!--\r\n    - Policies are applied in the order they appear.\r\n    - Position <base/> inside a section to inherit policies from the outer scope.\r\n    - Comments within policies are not preserved.\r\n-->\r\n<!-- Add policies as children to the <inbound>, <outbound>, <backend>, and <on-error> elements -->\r\n<policies>\r\n  <!-- Throttle, authorize, validate, cache, or transform the requests -->\r\n  <inbound>\r\n    <base />\r\n    <rewrite-uri template="/api/GetUpdatablePassesTrigger?passTypeIdentifier={passTypeIdentifier}&amp;deviceLibraryIdentifier={deviceLibraryIdentifier}" />\r\n  </inbound>\r\n  <!-- Control if and how the requests are forwarded to services  -->\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <!-- Customize the responses -->\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <!-- Handle exceptions and customize error responses  -->\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [
    service_nohomersapimgmt_name_no_homers_pass_aswa
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_get_updated_badge_policy 'Microsoft.ApiManagement/service/apis/operations/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa_get_updated_badge
  name: 'policy'
  properties: {
    value: '<!--\r\n    - Policies are applied in the order they appear.\r\n    - Position <base/> inside a section to inherit policies from the outer scope.\r\n    - Comments within policies are not preserved.\r\n-->\r\n<!-- Add policies as children to the <inbound>, <outbound>, <backend>, and <on-error> elements -->\r\n<policies>\r\n  <!-- Throttle, authorize, validate, cache, or transform the requests -->\r\n  <inbound>\r\n    <base />\r\n    <rewrite-uri template="/api/GetExistingPassTrigger?passTypeIdentifier={passTypeIdentifier}&amp;serialNumber={serialNumber}" />\r\n    <set-header name="X-Authorization" exists-action="append">\r\n      <value>@(context.Request.Headers.GetValueOrDefault("Authorization", ""))</value>\r\n    </set-header>\r\n  </inbound>\r\n  <!-- Control if and how the requests are forwarded to services  -->\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <!-- Customize the responses -->\r\n  <outbound>\r\n    <base />\r\n    <set-header name="Last-Modified" exists-action="append">\r\n      <value>@(context.Response.Headers.GetValueOrDefault("X-Last-Modified", ""))</value>\r\n    </set-header>\r\n  </outbound>\r\n  <!-- Handle exceptions and customize error responses  -->\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [
    service_nohomersapimgmt_name_no_homers_pass_aswa
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_log_policy 'Microsoft.ApiManagement/service/apis/operations/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa_log
  name: 'policy'
  properties: {
    value: '<!--\r\n    - Policies are applied in the order they appear.\r\n    - Position <base/> inside a section to inherit policies from the outer scope.\r\n    - Comments within policies are not preserved.\r\n-->\r\n<!-- Add policies as children to the <inbound>, <outbound>, <backend>, and <on-error> elements -->\r\n<policies>\r\n  <!-- Throttle, authorize, validate, cache, or transform the requests -->\r\n  <inbound>\r\n    <base />\r\n    <rewrite-uri template="/api/InsertLogTrigger" />\r\n  </inbound>\r\n  <!-- Control if and how the requests are forwarded to services  -->\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <!-- Customize the responses -->\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <!-- Handle exceptions and customize error responses  -->\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [
    service_nohomersapimgmt_name_no_homers_pass_aswa
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_register_pass_policy 'Microsoft.ApiManagement/service/apis/operations/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa_register_pass
  name: 'policy'
  properties: {
    value: '<!--\r\n    - Policies are applied in the order they appear.\r\n    - Position <base/> inside a section to inherit policies from the outer scope.\r\n    - Comments within policies are not preserved.\r\n-->\r\n<!-- Add policies as children to the <inbound>, <outbound>, <backend>, and <on-error> elements -->\r\n<policies>\r\n  <!-- Throttle, authorize, validate, cache, or transform the requests -->\r\n  <inbound>\r\n    <base />\r\n    <rewrite-uri template="/api/RegisterForUpdateNotificationsTrigger?deviceLibraryIdentifier={deviceLibraryIdentifier}&amp;passTypeIdentifier={passTypeIdentifier}&amp;serialNumber={serialNumber}" />\r\n    <set-header name="X-Authorization" exists-action="append">\r\n      <value>@(context.Request.Headers.GetValueOrDefault("Authorization", ""))</value>\r\n    </set-header>\r\n  </inbound>\r\n  <!-- Control if and how the requests are forwarded to services  -->\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <!-- Customize the responses -->\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <!-- Handle exceptions and customize error responses  -->\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [
    service_nohomersapimgmt_name_no_homers_pass_aswa
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_unregister_policy 'Microsoft.ApiManagement/service/apis/operations/policies@2024-06-01-preview' = {
  parent: service_nohomersapimgmt_name_no_homers_pass_aswa_unregister
  name: 'policy'
  properties: {
    value: '<!--\r\n    - Policies are applied in the order they appear.\r\n    - Position <base/> inside a section to inherit policies from the outer scope.\r\n    - Comments within policies are not preserved.\r\n-->\r\n<!-- Add policies as children to the <inbound>, <outbound>, <backend>, and <on-error> elements -->\r\n<policies>\r\n  <!-- Throttle, authorize, validate, cache, or transform the requests -->\r\n  <inbound>\r\n    <base />\r\n    <rewrite-uri template="/api/UnregisterPassTrigger?passTypeIdentifier={passTypeIdentifier}&amp;deviceLibraryIdentifier={deviceLibraryIdentifier}&amp;serialNumber={serialNumber}" />\r\n    <set-header name="X-Authorization" exists-action="append">\r\n      <value>@(context.Request.Headers.GetValueOrDefault("Authorization", ""))</value>\r\n    </set-header>\r\n  </inbound>\r\n  <!-- Control if and how the requests are forwarded to services  -->\r\n  <backend>\r\n    <base />\r\n  </backend>\r\n  <!-- Customize the responses -->\r\n  <outbound>\r\n    <base />\r\n  </outbound>\r\n  <!-- Handle exceptions and customize error responses  -->\r\n  <on-error>\r\n    <base />\r\n  </on-error>\r\n</policies>'
    format: 'xml'
  }
  dependsOn: [
    service_nohomersapimgmt_name_no_homers_pass_aswa
    service_nohomersapimgmt_name_resource
  ]
}

resource service_nohomersapimgmt_name_no_homers_pass_aswa_681386642c25ba18a00dbc38 'Microsoft.ApiManagement/service/products/apiLinks@2024-06-01-preview' = {
  parent: Microsoft_ApiManagement_service_products_service_nohomersapimgmt_name_no_homers_pass_aswa
  name: '681386642c25ba18a00dbc38'
  properties: {
    apiId: service_nohomersapimgmt_name_no_homers_pass_aswa.id
  }
  dependsOn: [
    service_nohomersapimgmt_name_resource
  ]
}
