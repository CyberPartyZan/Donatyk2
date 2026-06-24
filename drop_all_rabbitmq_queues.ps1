$base = "http://localhost:15672/api/queues"
$user = "guest"
$pass = "guest"

$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$user`:$pass"))

$queues = Invoke-RestMethod -Uri $base -Headers @{Authorization = "Basic $auth"}

foreach ($q in $queues) {
    $name = $q.name
    $vhost = [uri]::EscapeDataString($q.vhost)

    Invoke-RestMethod -Method Delete `
        -Uri "http://localhost:15672/api/queues/$vhost/$name" `
        -Headers @{Authorization = "Basic $auth"}

    Write-Host "Deleted $name"
}