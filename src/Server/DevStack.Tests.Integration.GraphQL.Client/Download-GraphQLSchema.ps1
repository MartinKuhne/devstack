#!/usr/bin/env pwsh
# Download GraphQL schema from the DevStack API
# Usage: ./Download-GraphQLSchema.ps1 [-Url <api-url>]

param(
    [string]$Url = "http://localhost:5000/graphql"
)

$outputFile = Join-Path $PSScriptRoot "graphql.schema.json"

Write-Host "Downloading GraphQL schema from $Url..."

# Use Introspection query to get the schema
$query = @"
{
  "__schema": {
    "queryType": { name: true },
    "mutationType": { name: true },
    "subscriptionType": { name: true },
    "types": {
      ...fullType
    },
    "directives": {
      ...directive
    }
  }
}

fragment fullType on __Type {
  kind
  name
  description
  fields(includeDeprecated: true) {
    name
    description
    args {
      ...inputValue
    }
    type {
      ...typeRef
    }
    isDeprecated
    deprecationReason
  }
  inputFields {
    ...inputValue
  }
  interfaces {
    ...typeRef
  }
  enumValues(includeDeprecated: true) {
    name
    description
    isDeprecated
    deprecationReason
  }
  possibleTypes {
    ...typeRef
  }
}

fragment inputValue on __InputValue {
  name
  description
  type { ...typeRef }
  defaultValue
}

fragment typeRef on __Type {
  kind
  name
  ofType {
    kind
    name
    ofType {
      kind
      name
      ofType {
        kind
        name
        ofType {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
              }
            }
          }
        }
      }
    }
  }
}

fragment directive on __Directive {
  name
  description
  locations
  args {
    ...inputValue
  }
}
"@

$body = @{ query = $query } | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $Url -Method Post -Body $body -ContentType "application/json"
    
    if ($response.data -and $response.data.__schema) {
        $response.data.__schema | ConvertTo-Json -Depth 100 | Out-File -FilePath $outputFile -Encoding utf8
        Write-Host "Schema saved to $outputFile"
    } else {
        Write-Error "Failed to retrieve schema: $($response.errors | ConvertTo-Json)"
        exit 1
    }
} catch {
    Write-Error "Failed to download schema: $_"
    exit 1
}
