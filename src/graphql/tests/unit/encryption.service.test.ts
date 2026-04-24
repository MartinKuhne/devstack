import { describe, it, expect } from 'vitest'
import { encryptApiKey, decryptApiKey } from '../../src/services/encryption.service'

describe('Encryption Service', () => {
  it('encrypts and decrypts a key roundtrip', async () => {
    const originalKey = 'sk-test-encryption-key-12345'
    const encrypted = await encryptApiKey(originalKey)
    const decrypted = await decryptApiKey(encrypted)
    expect(decrypted).toBe(originalKey)
  })

  it('different keys produce different ciphertext', async () => {
    const key1 = 'sk-key-one-12345'
    const key2 = 'sk-key-two-67890'
    const encrypted1 = await encryptApiKey(key1)
    const encrypted2 = await encryptApiKey(key2)
    expect(encrypted1).not.toBe(encrypted2)
  })

  it('encrypts with same key produce different ciphertext (IV randomization)', async () => {
    const key = 'sk-same-key-12345'
    const encrypted1 = await encryptApiKey(key)
    const encrypted2 = await encryptApiKey(key)
    expect(encrypted1).not.toBe(encrypted2)
  })

  it('handles empty string', async () => {
    const originalKey = ''
    const encrypted = await encryptApiKey(originalKey)
    const decrypted = await decryptApiKey(encrypted)
    expect(decrypted).toBe(originalKey)
  })

  it('handles long key', async () => {
    const longKey = 'x'.repeat(500)
    const encrypted = await encryptApiKey(longKey)
    const decrypted = await decryptApiKey(encrypted)
    expect(decrypted).toBe(longKey)
  })
})
