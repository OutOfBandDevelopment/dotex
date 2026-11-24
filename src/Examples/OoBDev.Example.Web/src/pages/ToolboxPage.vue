<script setup lang="ts">
import { ToolboxViewModel, hashAlgorithms, hmacAlgorithms, pipelineStepKeys } from './ToolboxViewModel';

const vm = new ToolboxViewModel();
const onInspectFile = (e: Event) => { vm.inspectFile.value = (e.target as HTMLInputElement).files?.[0] ?? null; };
</script>

<template>
  <h1>Toolbox</h1>
  <h2>Hash</h2>
  <p><label>Algorithm <select v-model="vm.hashAlgorithm.value"><option v-for="a in hashAlgorithms" :key="a">{{ a }}</option></select></label></p>
  <p><label>Text <input v-model="vm.hashText.value" size="50" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.hash.canExecute.value" @click="vm.hash.execute()">Hash</fluent-button></p>
  <p v-if="vm.hash.error.value" class="error">{{ vm.hash.error.value }}</p>
  <pre class="result" aria-label="Hash result">{{ vm.hashResult.value }}</pre>

  <h2>Sign a message (HMAC)</h2>
  <p><label>Algorithm <select v-model="vm.hmacAlgorithm.value"><option v-for="a in hmacAlgorithms" :key="a">{{ a }}</option></select></label></p>
  <p><label>Secret <input v-model="vm.secret.value" size="30" /></label></p>
  <p><label>Message <input v-model="vm.message.value" size="50" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.hmac.canExecute.value" @click="vm.hmac.execute()">Sign</fluent-button></p>
  <p v-if="vm.hmac.error.value" class="error">{{ vm.hmac.error.value }}</p>
  <pre class="result" aria-label="Signature">{{ vm.hmacResult.value }}</pre>

  <h2>Vector distance</h2>
  <p>Numbers separated by commas; both vectors need the same length.</p>
  <p><label>Vector A <input v-model="vm.vectorA.value" size="40" /></label></p>
  <p><label>Vector B <input v-model="vm.vectorB.value" size="40" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.distance.canExecute.value" @click="vm.distance.execute()">Compare</fluent-button></p>
  <p v-if="vm.distance.error.value" class="error">{{ vm.distance.error.value }}</p>
  <table v-if="vm.distances.value" class="grid" aria-label="Distances">
    <tbody>
      <tr><th>Cosine distance</th><td>{{ vm.distances.value.cosine.toFixed(6) }}</td></tr>
      <tr><th>Euclidean</th><td>{{ vm.distances.value.euclidean.toFixed(6) }}</td></tr>
      <tr><th>Dot product</th><td>{{ vm.distances.value.dotProduct.toFixed(6) }}</td></tr>
      <tr><th>Manhattan</th><td>{{ vm.distances.value.manhattan.toFixed(6) }}</td></tr>
    </tbody>
  </table>

  <h2>Calculator</h2>
  <p>Operators + - * / % ^ and parentheses; variables start with a capital letter (A, Rate2), and ! is factorial. Give their values below, one per line as name = value.</p>
  <p><label>Expression <input v-model="vm.expression.value" size="40" /></label></p>
  <p><label>Values <textarea v-model="vm.variablesText.value" rows="2" cols="30"></textarea></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.calculate.canExecute.value" @click="vm.calculate.execute()">Calculate</fluent-button></p>
  <p v-if="vm.calculate.error.value" class="error">{{ vm.calculate.error.value }}</p>
  <table v-if="vm.calculation.value" class="grid" aria-label="Calculation">
    <tbody>
      <tr><th>Result</th><td>{{ vm.calculation.value.result }}</td></tr>
      <tr><th>Optimized</th><td>{{ vm.calculation.value.optimized }}</td></tr>
      <tr><th>Variables</th><td>{{ vm.calculation.value.variables.join(', ') }}</td></tr>
    </tbody>
  </table>

  <h2>One-time code (TOTP)</h2>
  <p>Leave the secret empty to generate one. Enter a code to check it against the secret. This demo shows the secret; real systems keep it private.</p>
  <p><label>Secret <input v-model="vm.otpSecret.value" size="40" /></label></p>
  <p><label>Code to check <input v-model="vm.otpCode.value" size="10" /></label></p>
  <p><fluent-button appearance="primary" @click="vm.generateOtp.execute()">Get code</fluent-button></p>
  <p v-if="vm.generateOtp.error.value" class="error">{{ vm.generateOtp.error.value }}</p>
  <table v-if="vm.otp.value" class="grid" aria-label="One-time code">
    <tbody>
      <tr><th>Current code</th><td>{{ vm.otp.value.code }}</td></tr>
      <tr><th>Authenticator URI</th><td>{{ vm.otp.value.uri }}</td></tr>
      <tr v-if="vm.otp.value.valid !== null"><th>Code is valid</th><td>{{ vm.otp.value.valid ? 'yes' : 'no' }}</td></tr>
    </tbody>
  </table>

  <h2>Classic codecs</h2>
  <p>Convert text with Morse code, Roman numerals (numbers up to 3999999), Base32 or packed BCD (a decimal number to hex bytes and back).</p>
  <p>
    <label>Codec <select v-model="vm.codec.value"><option value="morse">Morse</option><option value="roman">Roman numerals</option><option value="base32">Base32</option><option value="bcd">Packed BCD (hex)</option></select></label>
    <label><input type="checkbox" v-model="vm.codecDecode.value" /> Decode</label>
  </p>
  <p><label>Text <input v-model="vm.codecValue.value" size="40" /></label></p>
  <p><fluent-button appearance="primary" @click="vm.convertCodec.execute()">Convert</fluent-button></p>
  <p v-if="vm.convertCodec.error.value" class="error">{{ vm.convertCodec.error.value }}</p>
  <p v-if="vm.codecResult.value" aria-label="Codec result"><code>{{ vm.codecResult.value }}</code></p>

  <h2>Signed token (JWS, HS256)</h2>
  <p>Sign a payload with a shared secret of at least 32 characters, or paste a token to read it. The payload is only trusted when a secret is given and the signature checks out.</p>
  <p><label>Payload <input v-model="vm.jwsPayload.value" size="40" /></label></p>
  <p><label>Secret <input v-model="vm.jwsSecret.value" size="40" /></label></p>
  <p><fluent-button appearance="primary" @click="vm.signJws.execute()">Sign</fluent-button></p>
  <p v-if="vm.signJws.error.value" class="error">{{ vm.signJws.error.value }}</p>
  <p><label>Token <input v-model="vm.jwsToken.value" size="60" /></label></p>
  <p><fluent-button @click="vm.readJws.execute()">Read and verify</fluent-button></p>
  <p v-if="vm.readJws.error.value" class="error">{{ vm.readJws.error.value }}</p>
  <table v-if="vm.jwsRead.value" class="grid" aria-label="Token contents">
    <tbody>
      <tr><th>Header</th><td>{{ vm.jwsRead.value.header }}</td></tr>
      <tr><th>Payload</th><td>{{ vm.jwsRead.value.payload }}</td></tr>
      <tr><th>Signature</th><td>{{ vm.jwsRead.value.verified === null ? 'not checked' : vm.jwsRead.value.verified ? 'valid' : 'invalid' }}</td></tr>
    </tbody>
  </table>

  <h2>File inspector</h2>
  <p>Pick a file (up to 5 MiB) to see its size, SHA-256, the content type found from its header and a hex dump of the first 256 bytes.</p>
  <p><label>File <input type="file" @change="onInspectFile" /></label></p>
  <p><fluent-button appearance="primary" :disabled="!vm.inspect.canExecute.value" @click="vm.inspect.execute()">Inspect</fluent-button></p>
  <p v-if="vm.inspect.error.value" class="error">{{ vm.inspect.error.value }}</p>
  <table v-if="vm.inspection.value" class="grid" aria-label="File inspection">
    <tbody>
      <tr><th>Size</th><td>{{ vm.inspection.value.length }} bytes</td></tr>
      <tr><th>SHA-256</th><td><code>{{ vm.inspection.value.sha256 }}</code></td></tr>
      <tr><th>Type</th><td>{{ vm.inspection.value.detectedContentType ?? "not recognized" }}</td></tr>
    </tbody>
  </table>
  <pre v-if="vm.inspection.value" aria-label="Hex dump">{{ vm.inspection.value.hexDump }}</pre>

  <h2>Pipeline builder</h2>
  <p>Chain byte transforms over text and see the bytes after every step (up to 20 steps).</p>
  <p><label>Text <input v-model="vm.pipelineValue.value" /></label></p>
  <p>
    <label>Step
      <select v-model="vm.pipelineChoice.value"><option v-for="k in pipelineStepKeys" :key="k" :value="k">{{ k }}</option></select>
    </label>
    <fluent-button :disabled="!vm.addPipelineStep.canExecute.value" @click="vm.addPipelineStep.execute()">Add step</fluent-button>
    <fluent-button :disabled="!vm.clearPipeline.canExecute.value" @click="vm.clearPipeline.execute()">Clear</fluent-button>
  </p>
  <p aria-label="Pipeline steps">{{ vm.pipelineSteps.value.length ? vm.pipelineSteps.value.join(' → ') : 'No steps yet.' }}</p>
  <p><fluent-button appearance="primary" :disabled="!vm.runPipeline.canExecute.value" @click="vm.runPipeline.execute()">Run</fluent-button></p>
  <p v-if="vm.runPipeline.error.value" class="error">{{ vm.runPipeline.error.value }}</p>
  <table v-if="vm.pipelineResults.value.length" class="grid" aria-label="Pipeline results">
    <thead><tr><th>Step</th><th>Bytes</th><th>Text</th><th>Hex</th></tr></thead>
    <tbody>
      <tr v-for="(r, i) in vm.pipelineResults.value" :key="i">
        <td>{{ r.step }}</td><td>{{ r.length }}</td><td>{{ r.text ?? '(binary)' }}</td><td><code>{{ r.hex.slice(0, 64) }}{{ r.hex.length > 64 ? '…' : '' }}</code></td>
      </tr>
    </tbody>
  </table>
</template>
