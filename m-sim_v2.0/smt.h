/*
 * smt.h: Contains SMT definitions
 *
 * Author: Joseph J. Sharkey <jsharke@cs.binghamton.edu>
 *
 */

#include "regs.h"
#include "sim.h"

#include "host.h"
#include "misc.h"
#include "machine.h"
#include "loader.h"
#include "regs.h"
#include "bitmap.h"

#ifndef CONTEXT_T
#define CONTEXT_T

/* the maximum number of concurent contexts permitted */
#define MAX_CONTEXTS 8


/* an entry in the create vector */
struct CV_link {
  struct ROB_entry *rs;                 /* creator's ROB entry */
  int odep_num;                         /* specific output operand */
};

struct fech_rec; //forward declairation
struct bpred_t; //forward declairation

typedef struct context_t{
  struct mem_t *mem;          /* the address space of this context */
  struct regs_t regs;         /* the architectural registers for this context */
  struct regs_t spec_regs;    /* the "speculative" regsiters for the context */

  /* program counter */
  md_addr_t pred_PC;
  md_addr_t recover_PC;
  /* fetch unit next fetch address */
  md_addr_t fetch_regs_PC;
  md_addr_t fetch_pred_PC;
  

  /* branch predictor - seperate for each thread */
  struct bpred_t *pred;

  /* load latency predictor - seperate for each thread */
  struct bpred_t *load_lat_pred;

  
  struct fetch_rec *fetch_data;	/* IFETCH -> DISPATCH inst queue */
  int fetch_num;		/* num entries in IF -> DIS queue */
  int fetch_tail, fetch_head;	/* head and tail pointers of queue */


  /* cycles until fetch issue resumes - used for modeling minimum branch mispredition penalty */
  unsigned fetch_issue_delay;

  /* the number of cycles to fast foward this thread */
  long long fastfwd_cnt;
  /* the nubmer of instructions left to fast forward */
  long long fastfwd_left;

  unsigned int ptrace_seq;		/* pipetrace sequence number */

  /* instruction count for the ICOUNT fetch policy */
  int icount;

  /* total number of instructions commited for this thread */
  long long sim_num_insn;

  BITMAP_TYPE(MD_NUM_IREGS, use_spec_R);
  BITMAP_TYPE(MD_NUM_FREGS, use_spec_F);
  BITMAP_TYPE(MD_NUM_FREGS, use_spec_C);


  /* the rename table for this context */
  int rename_table[MD_TOTAL_REGS];
  
  
  /* the create vector, NOTE: speculative copy on write storage provided
     for fast recovery during wrong path execute (see tracer_recover() for
     details on this process */
  BITMAP_TYPE(MD_TOTAL_REGS, use_spec_cv);
  struct CV_link create_vector[MD_TOTAL_REGS];
  struct CV_link spec_create_vector[MD_TOTAL_REGS];

  /* these arrays shadow the create vector an indicate when a register was
     last created */
  tick_t create_vector_rt[MD_TOTAL_REGS];
  tick_t spec_create_vector_rt[MD_TOTAL_REGS];

  /* Re-Order Buffer - organized as a circulatr queue */
  struct ROB_entry *ROB;		/* Re-order buffer */
  int ROB_head, ROB_tail;		/* ROB head and tail pointers */
  int ROB_num;			        /* num entries currently in ROB */


  /*
   * load/store queue (LSQ): holds loads and stores in program order, indicating
   * status of load/store access:
   *
   *   - issued: address computation complete, memory access in progress
   *   - completed: memory access has completed, stored value available
   *   - squashed: memory access was squashed, ignore this entry
   *
   * loads may execute when:
   *   1) register operands are ready, and
   *   2) memory operands are ready (no earlier unresolved store)
   *
   * loads are serviced by:
   *   1) previous store at same address in LSQ (hit latency), or
   *   2) data cache (hit latency + miss latency)
   *
   * stores may execute when:
   *   1) register operands are ready
   *
   * stores are serviced by:
   *   1) depositing store value into the load/store queue
   *   2) writing store value to the store buffer (plus tag check) at commit
   *   3) writing store buffer entry to data cache when cache is free
   *
   * NOTE: the load/store queue can bypass a store value to a load in the same
   *   cycle the store executes (using a bypass network), thus stores complete
   *   in effective zero time after their effective address is known
   */
  struct ROB_entry *LSQ;         /* load/store queue */
  int LSQ_head, LSQ_tail;          /* LSQ head and tail pointers */
  int LSQ_num;                     /* num entries currently in LSQ */
  

  int spec_mode;              /* TRUE or FALSE, is this context in "spec" mode */

  int id;                     /* thread id */
  FILE *infile, *outfile;     /* input(stdin) / output(stdout) redirects for the context */
  FILE *argfile;              /* the arg file specifying commandline containing arguments for this thread */

} context;

/* initalizes the context */
void init_context(context*, int);

#endif
