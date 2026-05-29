export interface AuthResponse {
  token: string
  email: string
  firstName: string
  lastName: string
  role: string
  tenantId: string
}

export interface Job {
  id: string
  title: string
  department: string
  location: string
  description: string | null
  status: string
  applicationCount: number
  createdAt: string
}

export interface Application {
  id: string
  jobId: string
  jobTitle: string
  candidateId: string
  candidateName: string
  candidateEmail: string
  stage: string
  coverNote: string | null
  createdAt: string
  updatedAt: string
  events: StageEvent[]
}

export interface StageEvent {
  fromStage: string | null
  toStage: string
  note: string | null
  createdAt: string
}
