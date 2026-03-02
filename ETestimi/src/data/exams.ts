export type ExamCategoryId = 'A' | 'B' | 'C' | 'D'

export interface ExamCategory {
  id: ExamCategoryId
  title: string
  description: string
}

export interface ExamQuestion {
  id: string
  text: string
  image?: string
  options: string[]
  correctOptionIndex: number
}

export interface ExamMeta {
  id: number
  title: string
}

export interface Exam {
  category: ExamCategoryId
  examId: number
  title: string
  rules: string[]
  questions: ExamQuestion[]
}

export const DEFAULT_QUESTION_IMAGE = '/images/cards/card-01.png'
export const EXAMS_PER_CATEGORY = 30

export const examCategories: ExamCategory[] = [
  {
    id: 'A',
    title: 'Kategoria A',
    description: 'Motoçikleta dhe mjete të lehta me dy rrota',
  },
  {
    id: 'B',
    title: 'Kategoria B',
    description: 'Automjete për pasagjerë dhe transport personal',
  },
  {
    id: 'C',
    title: 'Kategoria C',
    description: 'Kamionë dhe automjete të rënda për mallra',
  },
  {
    id: 'D',
    title: 'Kategoria D',
    description: 'Autobusë dhe automjete për transport pasagjerësh',
  },
]

const baseQuestions: Omit<ExamQuestion, 'id'>[] = [
  {
    text: 'Çfarë duhet të bëni kur afroheni te një vendkalim këmbësorësh me njerëz që presin?',
    image: '/images/cards/card-02.png',
    options: [
      'Rrit shpejtësinë për të kaluar para tyre',
      'Ngadalëso dhe përgatitu të ndalesh',
      'Përdor borinë dhe vazhdo',
      'Ndal vetëm natën',
    ],
    correctOptionIndex: 1,
  },
  {
    text: 'Kur është i detyrueshëm përdorimi i rripit të sigurimit gjatë drejtimit?',
    options: ['Vetëm në autostradë', 'Vetëm në qytet', 'Gjithmonë, përveç rasteve të përjashtuara me ligj', 'Vetëm kur ka polici afër'],
    correctOptionIndex: 2,
  },
  {
    text: 'Një vijë e vazhduar në qendër të rrugës do të thotë:',
    image: '/images/cards/card-03.png',
    options: [
      'Parakalimi lejohet gjithmonë',
      'Parkimi është i detyrueshëm',
      'Kalimi i vijës ndalohet, përveç rasteve kur lejohet me ligj',
      'Rruga është njëkahëshe',
    ],
    correctOptionIndex: 2,
  },
  {
    text: 'Cili është veprimi më i sigurt kur humbisni kthesën tuaj?',
    options: [
      'Ndal menjëherë në korsi',
      'Ec mbrapa për ta arritur',
      'Vazhdo i sigurt dhe ndrysho itinerarin',
      'Kthehu mes trafikut pa sinjalizuar',
    ],
    correctOptionIndex: 2,
  },
  {
    text: 'Nëse shikueshmëria është e dobët për shkak të mjegullës, duhet të:',
    options: [
      'Përdorësh dritat e gjata vazhdimisht',
      'Ulësh shpejtësinë dhe rritësh distancën e ndjekjes',
      'Ngasësh afër mjetit përpara për orientim',
      'Fikësh dritat për të shmangur reflektimin',
    ],
    correctOptionIndex: 1,
  },
]

export const isValidCategory = (value: string): value is ExamCategoryId =>
  examCategories.some((category) => category.id === value)

export const getCategoryById = (categoryId: string) =>
  examCategories.find((category) => category.id === categoryId)

export const getExamsByCategory = (categoryId: ExamCategoryId): ExamMeta[] =>
  (void categoryId,
  Array.from({ length: EXAMS_PER_CATEGORY }, (_, index) => ({
    id: index + 1,
    title: `Testi ${index + 1}`,
  })))

const createQuestions = (category: ExamCategoryId, examId: number): ExamQuestion[] =>
  baseQuestions.map((question, index) => ({
    ...question,
    id: `${category}-${examId}-${index + 1}`,
    text: `${question.text} (${category}-${examId})`,
  }))

export const getExam = (categoryId: string, examId: number): Exam | null => {
  if (!isValidCategory(categoryId)) {
    return null
  }

  if (Number.isNaN(examId) || examId < 1 || examId > EXAMS_PER_CATEGORY) {
    return null
  }

  return {
    category: categoryId,
    examId,
    title: `${categoryId} - Testi ${examId}`,
    rules: [
      'Lexoni me kujdes çdo pyetje para se të zgjidhni përgjigjen.',
      'Zgjidhni vetëm një përgjigje për secilën pyetje.',
      'Mos rifreskoni faqen gjatë një tentative aktive.',
      'Dorëzojeni testin në fund për të parë përgjigjet e sakta dhe të gabuara.',
    ],
    questions: createQuestions(categoryId, examId),
  }
}
