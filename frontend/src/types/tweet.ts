export interface Tweet {
  id: string
  authorId: string
  authorUsername: string
  text: string
  parentId: string | null
  imageUrl: string | null
  createdAt: string
  likeCount: number
  likedByViewer: boolean
  retweetCount: number
  retweetedByViewer: boolean
  isRetweet?: boolean
  retweetedByUsername?: string
}

export interface CreateTweetRequest {
  text: string
  parentId?: string
  imageUrl?: string
}
