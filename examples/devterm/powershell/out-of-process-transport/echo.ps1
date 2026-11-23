# A transport plugin: an echo "device". Speaks newline-delimited JSON on stdin and stdout (docs/design/Plugins/out-of-process-plugins.md).
$in = [Console]::In
while ($line = $in.ReadLine()) {
    $m = $line | ConvertFrom-Json
    switch ($m.type) {
        'hello' { '{"type":"hello","name":"ps-echo","protocol":1}' }
        'open'  { '{"type":"opened"}' }
        'write' { '{"type":"data","hex":"' + $m.hex + '"}' }
        'close' { exit }
    }
}
