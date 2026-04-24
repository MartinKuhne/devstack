import type { Pool } from 'pg'

declare global {
  // eslint-disable-next-line no-var, vars-on-top
  var __pgPool__: Pool | undefined
}
