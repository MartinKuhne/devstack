import crypto from 'node:crypto'

const ALGORITHM = 'aes-256-cbc'
const KEY = process.env.ENCRYPTION_KEY ?? crypto.randomBytes(32).toString('hex')
const IV_LENGTH = 16

export async function encryptApiKey(key: string): Promise<string> {
  const iv = crypto.randomBytes(IV_LENGTH)
  const cipher = crypto.createCipheriv(ALGORITHM, Buffer.from(KEY, 'hex'), iv)
  let encrypted = cipher.update(key, 'utf8', 'hex')
  encrypted += cipher.final('hex')
  return `${iv.toString('hex')}:${encrypted}`
}

export async function decryptApiKey(encryptedKey: string): Promise<string> {
  const [ivHex, encrypted] = encryptedKey.split(':')
  const iv = Buffer.from(ivHex, 'hex')
  const decipher = crypto.createDecipheriv(ALGORITHM, Buffer.from(KEY, 'hex'), iv)
  let decrypted = decipher.update(encrypted, 'hex', 'utf8')
  decrypted += decipher.final('utf8')
  return decrypted
}
