import { describe, it, expect } from 'vitest'
import {
  isValidDeliverableTransition,
  isValidAgentTaskTransition,
} from '../../src/services/transition.service'

describe('Deliverable Transition Service', () => {
  describe('isValidDeliverableTransition', () => {
    it('allows Draft -> Planning', () => {
      expect(isValidDeliverableTransition('DRAFT', 'PLANNING')).toBe(true)
    })

    it('allows Draft -> Rejected', () => {
      expect(isValidDeliverableTransition('DRAFT', 'REJECTED')).toBe(true)
    })

    it('allows Planning -> Ready', () => {
      expect(isValidDeliverableTransition('PLANNING', 'READY')).toBe(true)
    })

    it('allows Planning -> Rejected', () => {
      expect(isValidDeliverableTransition('PLANNING', 'REJECTED')).toBe(true)
    })

    it('allows Ready -> InProgress', () => {
      expect(isValidDeliverableTransition('READY', 'INPROGRESS')).toBe(true)
    })

    it('allows Ready -> Rejected', () => {
      expect(isValidDeliverableTransition('READY', 'REJECTED')).toBe(true)
    })

    it('allows InProgress -> CodeComplete', () => {
      expect(isValidDeliverableTransition('INPROGRESS', 'CODECOMPLETE')).toBe(true)
    })

    it('allows InProgress -> Failed', () => {
      expect(isValidDeliverableTransition('INPROGRESS', 'FAILED')).toBe(true)
    })

    it('allows InProgress -> Rejected', () => {
      expect(isValidDeliverableTransition('INPROGRESS', 'REJECTED')).toBe(true)
    })

    it('allows CodeComplete -> Testing', () => {
      expect(isValidDeliverableTransition('CODECOMPLETE', 'TESTING')).toBe(true)
    })

    it('allows CodeComplete -> Rejected', () => {
      expect(isValidDeliverableTransition('CODECOMPLETE', 'REJECTED')).toBe(true)
    })

    it('allows Testing -> NeedsReview', () => {
      expect(isValidDeliverableTransition('TESTING', 'NEEDSREVIEW')).toBe(true)
    })

    it('allows Testing -> Failed', () => {
      expect(isValidDeliverableTransition('TESTING', 'FAILED')).toBe(true)
    })

    it('allows Testing -> Rejected', () => {
      expect(isValidDeliverableTransition('TESTING', 'REJECTED')).toBe(true)
    })

    it('allows NeedsReview -> InProgress', () => {
      expect(isValidDeliverableTransition('NEEDSREVIEW', 'INPROGRESS')).toBe(true)
    })

    it('allows NeedsReview -> Done', () => {
      expect(isValidDeliverableTransition('NEEDSREVIEW', 'DONE')).toBe(true)
    })

    it('allows NeedsReview -> Rejected', () => {
      expect(isValidDeliverableTransition('NEEDSREVIEW', 'REJECTED')).toBe(true)
    })

    it('rejects Done -> any', () => {
      expect(isValidDeliverableTransition('DONE', 'DRAFT')).toBe(false)
      expect(isValidDeliverableTransition('DONE', 'INPROGRESS')).toBe(false)
    })

    it('rejects Failed -> any', () => {
      expect(isValidDeliverableTransition('FAILED', 'DRAFT')).toBe(false)
      expect(isValidDeliverableTransition('FAILED', 'INPROGRESS')).toBe(false)
    })

    it('rejects Rejected -> any', () => {
      expect(isValidDeliverableTransition('REJECTED', 'DRAFT')).toBe(false)
      expect(isValidDeliverableTransition('REJECTED', 'PLANNING')).toBe(false)
    })

    it('rejects invalid transitions', () => {
      expect(isValidDeliverableTransition('DRAFT', 'READY')).toBe(false)
      expect(isValidDeliverableTransition('DRAFT', 'INPROGRESS')).toBe(false)
      expect(isValidDeliverableTransition('PLANNING', 'DONE')).toBe(false)
      expect(isValidDeliverableTransition('READY', 'DONE')).toBe(false)
      expect(isValidDeliverableTransition('INPROGRESS', 'DONE')).toBe(false)
      expect(isValidDeliverableTransition('INPROGRESS', 'TESTING')).toBe(false)
      expect(isValidDeliverableTransition('CODECOMPLETE', 'DONE')).toBe(false)
      expect(isValidDeliverableTransition('CODECOMPLETE', 'INPROGRESS')).toBe(false)
      expect(isValidDeliverableTransition('TESTING', 'INPROGRESS')).toBe(false)
    })
  })
})

describe('AgentTask Transition Service', () => {
  describe('isValidAgentTaskTransition', () => {
    it('allows Ready -> InProgress', () => {
      expect(isValidAgentTaskTransition('READY', 'INPROGRESS')).toBe(true)
    })

    it('allows Ready -> Rejected', () => {
      expect(isValidAgentTaskTransition('READY', 'REJECTED')).toBe(true)
    })

    it('allows InProgress -> Done', () => {
      expect(isValidAgentTaskTransition('INPROGRESS', 'DONE')).toBe(true)
    })

    it('allows InProgress -> Failed', () => {
      expect(isValidAgentTaskTransition('INPROGRESS', 'FAILED')).toBe(true)
    })

    it('allows InProgress -> NeedsReview', () => {
      expect(isValidAgentTaskTransition('INPROGRESS', 'NEEDSREVIEW')).toBe(true)
    })

    it('allows InProgress -> Rejected', () => {
      expect(isValidAgentTaskTransition('INPROGRESS', 'REJECTED')).toBe(true)
    })

    it('allows NeedsReview -> InProgress', () => {
      expect(isValidAgentTaskTransition('NEEDSREVIEW', 'INPROGRESS')).toBe(true)
    })

    it('allows NeedsReview -> Done', () => {
      expect(isValidAgentTaskTransition('NEEDSREVIEW', 'DONE')).toBe(true)
    })

    it('allows NeedsReview -> Rejected', () => {
      expect(isValidAgentTaskTransition('NEEDSREVIEW', 'REJECTED')).toBe(true)
    })

    it('rejects Done -> any', () => {
      expect(isValidAgentTaskTransition('DONE', 'READY')).toBe(false)
      expect(isValidAgentTaskTransition('DONE', 'INPROGRESS')).toBe(false)
    })

    it('rejects Failed -> any', () => {
      expect(isValidAgentTaskTransition('FAILED', 'READY')).toBe(false)
      expect(isValidAgentTaskTransition('FAILED', 'INPROGRESS')).toBe(false)
    })

    it('rejects Rejected -> any', () => {
      expect(isValidAgentTaskTransition('REJECTED', 'READY')).toBe(false)
      expect(isValidAgentTaskTransition('REJECTED', 'INPROGRESS')).toBe(false)
    })

    it('rejects invalid transitions', () => {
      expect(isValidAgentTaskTransition('READY', 'DONE')).toBe(false)
      expect(isValidAgentTaskTransition('INPROGRESS', 'READY')).toBe(false)
    })
  })
})
