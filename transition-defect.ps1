$ErrorActionPreference = 'Stop'

$query = @"
mutation {
    transitionDefectStatus(input: {
        id: "4d086fe0-db70-4233-b61f-34fbc1d1cbdc"
        targetStatus: Done
        actor: "user"
    }) {
        defect {
            id
            status
        }
        errors
    }
}
"@

$body = @{query = $query} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri 'http://localhost:8087/graphql' -Method Post -Body $body -ContentType 'application/json' -UseBasicParsing
    Write-Host "Success:"
    Write-Host $response.Content
}
catch {
    Write-Host "Error Status: $($_.Exception.Response.StatusCode)"
    if ($_.ErrorDetails) {
        Write-Host "Error Response:"
        Write-Host $_.ErrorDetails.Message
    }
    else {
        Write-Host "Error Message: $($_.Exception.Message)"
    }
}
