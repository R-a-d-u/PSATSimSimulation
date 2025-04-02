/*
 * sim-outorder:
 *
 * Copyright (c) 2005 Joseph J. Sharkey <jsharke@cs.binghamton.edu>
 * Copyright (c) 1994-2003 Todd M. Austin, Doug Berger, SimpleScalar, LLC.
 * 
 * This program is licensed under the GNU General Public License version 2.
 *
 * NOTE: See the README file for a full revision history.
 *
 * Additional documentation is provided in the included presentation.pdf.
 *
 * Written by:
 * 
 * - Joseph J. Sharkey <jsharke@cs.binghamton.edu>, September 2005
 *
 *   Restructured the out-of-order core to support cycle accurate modeling of
 *   independent structures for the re-order buffer, issue queue, register file
 *   and register renaming.
 *   Added support for Simultaneous Multithreading (SMT). See README for details.
 *   Only supports Alpha ISA now, to simplify the code (ie no support for longs and doubles)
 *
 * - Todd M. Austin, Doug Berger, SimpleScalar, LLC.
 *
 *   Original SimpleScalar 3.0d implementation, without cycle accurate modeling
 *   for some processor structures
 *
 * sim-outorder.c:
 *
 * SimpleScalar(TM) Tool Suite
 * Copyright (C) 1994-2003 by Todd M. Austin, Ph.D. and SimpleScalar, LLC.
 * All Rights Reserved. 
 *
 * Copyright (C) 2005 Joseph J Sharkey <jsharke@cs.binghamton.edu>
 *
 * THIS IS A LEGAL DOCUMENT, BY USING SIMPLESCALAR,
 * YOU ARE AGREEING TO THESE TERMS AND CONDITIONS.
 * 
 * No portion of this work may be used by any commercial entity, or for any
 * commercial purpose, without the prior, written permission of SimpleScalar,
 * LLC (info@simplescalar.com). Nonprofit and noncommercial use is permitted
 * as described below.
 * 
 * 1. SimpleScalar is provided AS IS, with no warranty of any kind, express
 * or implied. The user of the program accepts full responsibility for the
 * application of the program and the use of any results.
 * 
 * 2. Nonprofit and noncommercial use is encouraged. SimpleScalar may be
 * downloaded, compiled, executed, copied, and modified solely for nonprofit,
 * educational, noncommercial research, and noncommercial scholarship
 * purposes provided that this notice in its entirety accompanies all copies.
 * Copies of the modified software can be delivered to persons who use it
 * solely for nonprofit, educational, noncommercial research, and
 * noncommercial scholarship purposes provided that this notice in its
 * entirety accompanies all copies.
 * 
 * 3. ALL COMMERCIAL USE, AND ALL USE BY FOR PROFIT ENTITIES, IS EXPRESSLY
 * PROHIBITED WITHOUT A LICENSE FROM SIMPLESCALAR, LLC (info@simplescalar.com).
 * 
 * 4. No nonprofit user may place any restrictions on the use of this software,
 * including as modified by the user, by any other authorized user.
 * 
 * 5. Noncommercial and nonprofit users may distribute copies of SimpleScalar
 * in compiled or executable form as set forth in Section 2, provided that
 * either: (A) it is accompanied by the corresponding machine-readable source
 * code, or (B) it is accompanied by a written offer, with no time limit, to
 * give anyone a machine-readable copy of the corresponding source code in
 * return for reimbursement of the cost of distribution. This written offer
 * must permit verbatim duplication by anyone, or (C) it is distributed by
 * someone who received only the executable form, and is accompanied by a
 * copy of the written offer of source code.
 * 
 * 6. SimpleScalar was developed by Todd M. Austin, Ph.D. The tool suite is
 * currently maintained by SimpleScalar LLC (info@simplescalar.com). US Mail:
 * 2395 Timbercrest Court, Ann Arbor, MI 48105.
 * 
 * Copyright (C) 1994-2003 by Todd M. Austin, Ph.D. and SimpleScalar, LLC.
 */


#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <assert.h>
#include <signal.h>

#include "host.h"
#include "misc.h"
#include "machine.h"
#include "regs.h"
#include "memory.h"
#include "cache.h"
#include "loader.h"
#include "syscall.h"
#include "bpred.h"
#include "resource.h"
#include "bitmap.h"
#include "options.h"
#include "eval.h"
#include "stats.h"
#include "ptrace.h"
#include "dlite.h"
#include "sim.h"
#include "smt.h"

/* added for Wattch */
#include "power.h"

/*
 * This file implements a very detailed out-of-order issue superscalar
 * processor with a two-level memory system and speculative execution support.
 * This simulator is a performance simulator, tracking the latency of all
 * pipeline operations.
 */


/*
 * simulator options
 */


/**************** SMT Options *******************/
/* the number of contexts present in the simulator */
int num_contexts = 0;

/* the actual contexts - (see smt.h for details) */
context contexts[MAX_CONTEXTS];

/***************************************/

/* maximum number of inst's to execute */
static unsigned int max_insts;

/* number of insts skipped before timing starts */
static int fastfwd_count;

/* pipeline trace range and output filename */
static int ptrace_nelt = 0;
static char *ptrace_opts[2];

/* instruction fetch queue size (in insts) */
static int ifq_size;

/* speed of front-end of machine relative to execution core */
static int fetch_speed;

/* branch predictor type {nottaken|taken|perfect|bimod|2lev} */
static char *pred_type;

/* bimodal predictor config (<table_size>) */
static int bimod_nelt = 1;
int bimod_config[1] =
  { /* bimod tbl size */2048 };

/* 2-level predictor config (<l1size> <l2size> <hist_size> <xor>) */
static int twolev_nelt = 4;
int twolev_config[4] =
  { /* l1size */1, /* l2size */1024, /* hist */8, /* xor */FALSE};

/* combining predictor config (<meta_table_size> */
static int comb_nelt = 1;
int comb_config[1] =
  { /* meta_table_size */1024 };

/* return address stack (RAS) size */
int ras_size = 8;

/* BTB predictor config (<num_sets> <associativity>) */
static int btb_nelt = 2;
int btb_config[2] =
  { /* nsets */512, /* assoc */4 };

/* instruction decode B/W (insts/cycle) */
int decode_width;

/* instruction issue B/W (insts/cycle) */
int issue_width;

/* run pipeline with in-order issue */
static int inorder_issue;

/* issue instructions down wrong execution paths */
static int include_spec = TRUE;

/* instruction commit B/W (insts/cycle) */
int commit_width;

/* re-order buffer (ROB) size */
int ROB_size = 8;

/* load/store queue (LSQ) size */
int LSQ_size = 4;

/* l1 data cache config, i.e., {<config>|none} */
static char *cache_dl1_opt;

/* l1 data cache hit latency (in cycles) */
static int cache_dl1_lat;

/* l2 data cache config, i.e., {<config>|none} */
static char *cache_dl2_opt;

/* l2 data cache hit latency (in cycles) */
static int cache_dl2_lat;

/* l1 instruction cache config, i.e., {<config>|dl1|dl2|none} */
static char *cache_il1_opt;

/* l1 instruction cache hit latency (in cycles) */
static int cache_il1_lat;

/* l2 instruction cache config, i.e., {<config>|dl1|dl2|none} */
static char *cache_il2_opt;

/* l2 instruction cache hit latency (in cycles) */
static int cache_il2_lat;

/* flush caches on system calls */
static int flush_on_syscalls;

/* convert 64-bit inst addresses to 32-bit inst equivalents */
static int compress_icache_addrs;

/* memory access latency (<first_chunk> <inter_chunk>) */
static int mem_nelt = 2;
static int mem_lat[2] =
  { /* lat to first chunk */100, /* lat between remaining chunks */2 };

/* memory access bus width (in bytes) */
static int mem_bus_width;

/* instruction TLB config, i.e., {<config>|none} */
static char *itlb_opt;

/* data TLB config, i.e., {<config>|none} */
static char *dtlb_opt;

/* inst/data TLB miss latency (in cycles) */
static int tlb_miss_lat;

/************** LOAD-LATENCY PREDICTOR CONFIGURATION ****************/
/* load-latency predictor type {nottaken|taken|perfect|bimod|2lev} */
static char *cpred_type;

/* bimodal predictor config (<table_size>) */
static int cbimod_nelt = 1;
int cbimod_config[1] =
  { /* bimod tbl size */2048 };

/* 2-level predictor config (<l1size> <l2size> <hist_size> <xor>) */
static int ctwolev_nelt = 4;
int ctwolev_config[4] =
  { /* l1size */1, /* l2size */1024, /* hist */8, /* xor */FALSE};

/* combining predictor config (<meta_table_size> */
static int ccomb_nelt = 1;
int ccomb_config[1] =
  { /* meta_table_size */1024 };

/* return address stack (RAS) size */
int cras_size = 0;

/* BTB predictor config (<num_sets> <associativity>) */
static int cbtb_nelt = 2;
int cbtb_config[2] =
  { /* nsets */512, /* assoc */4 };
/************** LOAD-LATENCY PREDICTOR CONFIGURATION ****************/

/* total number of integer ALU's available */
int res_ialu;

/* total number of integer multiplier/dividers available */
int res_imult;

/* total number of memory system ports available (to CPU) */
int res_memport;

/* total number of floating point ALU's available */
int res_fpalu;

/* total number of floating point multiplier/dividers available */
int res_fpmult;

/* options for Wattch */
int data_width = 64;

/* total size of the issue queue */
int iq_size;

/* ALPHA 21264 SQUASH RECOVERY or PERFECT */
static char *recovery_model;

/* use icount fetch? */
static char *fetch_policy;

/* size of the physical register file*/
/* (INT and FP */
int rf_size;

/* PIPELINE STAGES */
static int FETCH_RENAME_DELAY = 4;
static int RENAME_DISPATCH_DELAY = 1;

/* number of cycles between issue and execute (includes RF access) */
static int ISSUE_EXEC_DELAY = 1;
static long long INF = 999999999;


/* static power model results */
extern power_result_type power;
static int print_power_stats = TRUE;

/* counters added for Wattch */
counter_t rename_access=0;
counter_t bpred_access=0;
counter_t window_access=0;
counter_t lsq_access=0;
counter_t regfile_access=0;
counter_t icache_access=0;
counter_t dcache_access=0;
counter_t dcache2_access=0;
counter_t alu_access=0;
counter_t ialu_access=0;
counter_t falu_access=0;
counter_t resultbus_access=0;

counter_t window_preg_access=0;
counter_t window_selection_access=0;
counter_t window_wakeup_access=0;
counter_t lsq_store_data_access=0;
counter_t lsq_load_data_access=0;
counter_t lsq_preg_access=0;
counter_t lsq_wakeup_access=0;

counter_t window_total_pop_count_cycle=0;
counter_t window_num_pop_count_cycle=0;
counter_t lsq_total_pop_count_cycle=0;
counter_t lsq_num_pop_count_cycle=0;
counter_t regfile_total_pop_count_cycle=0;
counter_t regfile_num_pop_count_cycle=0;
counter_t resultbus_total_pop_count_cycle=0;
counter_t resultbus_num_pop_count_cycle=0;

/* text-based stat profiles */
#define MAX_PCSTAT_VARS 8
static int pcstat_nelt = 0;
static char *pcstat_vars[MAX_PCSTAT_VARS];

/* convert 64-bit inst text addresses to 32-bit inst equivalents */
#define IACOMPRESS(A)		(A)
#define ISCOMPRESS(SZ)		(SZ)

/*
 * functional unit resource configuration
 */

/* resource pool indices, NOTE: update these if you change FU_CONFIG */
#define FU_IALU_INDEX			0
#define FU_IMULT_INDEX			1
#define FU_MEMPORT_INDEX		2
#define FU_FPALU_INDEX			3
#define FU_FPMULT_INDEX			4

/* resource pool definition, NOTE: update FU_*_INDEX defs if you change this */
struct res_desc fu_config[] = {
  {
    "integer-ALU",
    4,
    0,
    {
      { IntALU, 1, 1 }
    }
  },
  {
    "integer-MULT/DIV",
    1,
    0,
    {
      { IntMULT, 3, 1 },
      { IntDIV, 20, 19 }
    }
  },
  {
    "memory-port",
    2,
    0,
    {
      { RdPort, 1, 1 },
      { WrPort, 1, 1 }
    }
  },
  {
    "FP-adder",
    4,
    0,
    {
      { FloatADD, 2, 1 },
      { FloatCMP, 2, 1 },
      { FloatCVT, 2, 1 }
    }
  },
  {
    "FP-MULT/DIV",
    1,
    0,
    {
      { FloatMULT, 4, 1 },
      { FloatDIV, 12, 12 },
      { FloatSQRT, 24, 24 }
    }
  },
};


/*
 * simulator stats
 */
/* SLIP variable */
static counter_t sim_slip = 0;

/* total number of instructions executed */
static counter_t sim_total_insn = 0;

/* total number of memory references committed */
static counter_t sim_num_refs = 0;

/* total number of memory references executed */
static counter_t sim_total_refs = 0;

/* total number of loads committed */
static counter_t sim_num_loads = 0;

/* total number of loads executed */
static counter_t sim_total_loads = 0;

/* total number of branches committed */
static counter_t sim_num_branches = 0;

/* total number of branches executed */
static counter_t sim_total_branches = 0;

/* cycle counter */
static tick_t sim_cycle = 0;

static tick_t last_commit_cycle = 0;

#define COMMIT_TIMEOUT 100000

/* total non-speculative bogus addresses seen (debug var) */
static counter_t sim_invalid_addrs;

/*
 * simulator state variables
 */

/* instruction sequence counter, used to assign unique id's to insts */
static unsigned int inst_seq = 0;

/* perfect prediction enabled */
static int pred_perfect = FALSE;

/* speculative bpred-update enabled */
static char *bpred_spec_opt;
static enum { spec_ID, spec_WB, spec_CT } bpred_spec_update;

/* level 1 instruction cache, entry level instruction cache */
struct cache_t *cache_il1;

/* level 1 instruction cache */
struct cache_t *cache_il2;

/* level 1 data cache, entry level data cache */
struct cache_t *cache_dl1;

/* level 2 data cache */
struct cache_t *cache_dl2;

/* instruction TLB */
struct cache_t *itlb;

/* data TLB */
struct cache_t *dtlb;

/* functional unit resource pool */
static struct res_pool *fu_pool = NULL;


/************** REGISTER RENAMING STRUCTURES *********************/

/*
 * holds the current state for a register
 */
enum reg_state {
  REG_FREE = 0, /* register is free (not allocated to any instruction) */
  REG_ALLOC,    /* register has been allocated, but not written to yet */
  REG_WB,       /* register has been written to, but not committed */
  REG_ARCH      /* register has been committed to the architectural state */
};

/*
 * A physical register - only contains state for now
 */
struct physreg_t{
  enum reg_state state; /* the state the register is currently in */
  tick_t ready;         /* earliest cycle in which the data will be available for read off bypass network */
  tick_t spec_ready;    /* earliest cycle instructions dependant on this register should issue (speculative on loads */
  long long alloc_cycle;
};

/*
 * The integer physical register file
 */
struct physreg_t* int_reg_file;

/*
 * The floating-point physical register file
 */
struct physreg_t* fp_reg_file;

/*
 * Indicates the type of a register
 */
enum reg_type {
  REG_NONE = 0, /* No register */
  REG_INT,      /* Integer register */
  REG_FP        /* Floating Point register */
};


/*
 *  Determines if a free physical register exists
 *  of the specified type
 */
static int find_free_physreg(enum reg_type type){
  if(type == REG_INT){
    int i;
    for(i = 0; i<rf_size; i++){
      if(int_reg_file[i].state == REG_FREE)
	return i;
    }
    return -1;
  }else if(type == REG_FP){
    int i;
    for(i = 0; i<rf_size; i++){
      if(fp_reg_file[i].state == REG_FREE)
	return i;
    }
    return -1;
  }
  assert(FALSE);
  return -1;
}

/* forward declairation */
struct ROB_entry;

/* 
 * forward declairation
 * Allocates physical registers
 */
static int alloc_physreg(struct ROB_entry* rob_entry);

//NOTE: Rename tables are kept per-thread in the context
//See smt.h for details

/*
 * Contains a set of registers used by an instruction
 * Indicates the types of each of the sources and destination
 */
struct reg_set{
  enum reg_type src1;  /* type for source 1*/
  enum reg_type src2;  /* type for source 2*/
  enum reg_type dest;  /* type for the destination*/
  int load;            /* is a load? */
  int store;           /* is a store? */
};

/*
 * forward declairation
 * Used to determine the set of registers needed for an instruction with a specific op-code
 */
static void get_reg_set(struct reg_set* my_regs, enum md_opcode op);

/**************************************************************/






/*************** SMT **************************/

/* marks an IQ entry type as free or allocated */
enum iq_entry{
  IQ_ENTRY_FREE = 0,  /* the entry is free */
  IQ_ENTRY_ALLOC,     /* the entry is allocated */
};

/* actual IQ */
static enum iq_entry *iq;

/*
 * Frees an issue queue entry
 */
static void free_iq_entry(int entry_num){
  assert(entry_num >= 0);

  assert(iq[entry_num] == IQ_ENTRY_ALLOC);
  iq[entry_num] = IQ_ENTRY_FREE;
}

/*
 * Locates the position of a free issue queue entry
 */
static int find_iq_entry(){
  int i;

  /* try to find an available entry */
  for(i=0;i<iq_size;i++){
    if(iq[i] == IQ_ENTRY_FREE){
      /* found a free entry */
      return i;
    }
  }
  /* didn't find an entry */
  return -1;
}

/*
 * Allocates an issue queue entry
 */
static int alloc_iq_entry(){
  /* find the location of a free IQ entry */
  int i = find_iq_entry();

  /* if no free entry exists, return -1 */
  if(i < 0){
    return -1;
  }

  assert(iq[i] == IQ_ENTRY_FREE);
  iq[i] = IQ_ENTRY_ALLOC;

  return i;
}

/**********************************************/


/* text-based stat profiles */
static struct stat_stat_t *pcstat_stats[MAX_PCSTAT_VARS];
static counter_t pcstat_lastvals[MAX_PCSTAT_VARS];
static struct stat_stat_t *pcstat_sdists[MAX_PCSTAT_VARS];

/* wedge all stat values into a counter_t */
#define STATVAL(STAT)							\
  ((STAT)->sc == sc_int							\
   ? (counter_t)*((STAT)->variant.for_int.var)			\
   : ((STAT)->sc == sc_uint						\
      ? (counter_t)*((STAT)->variant.for_uint.var)		\
      : ((STAT)->sc == sc_counter					\
	 ? *((STAT)->variant.for_counter.var)				\
	 : (panic("bad stat class"), 0))))


/* memory access latency, assumed to not cross a page boundary */
static unsigned int			/* total latency of access */
mem_access_latency(int blk_sz)		/* block size accessed */
{
  int chunks = (blk_sz + (mem_bus_width - 1)) / mem_bus_width;

  assert(chunks > 0);

  return (/* first chunk latency */mem_lat[0] +
	  (/* remainder chunk latency */mem_lat[1] * (chunks - 1)));
}


/*
 * cache miss handlers
 */

/* l1 data cache l1 block miss handler function */
static unsigned int			/* latency of block access */
dl1_access_fn(enum mem_cmd cmd,		/* access cmd, Read or Write */
	      md_addr_t baddr,		/* block address to access */
	      int bsize,		/* size of block to access */
	      struct cache_blk_t *blk,	/* ptr to block in upper level */
	      tick_t now,		/* time of access */
	      int context_id)           /* conext_id for the access */
{
  unsigned int lat;

  if (cache_dl2)
    {
      /* access next level of data cache hierarchy */
      lat = cache_access(cache_dl2, cmd, baddr, context_id, NULL, bsize,
			 /* now */now, /* pudata */NULL, /* repl addr */NULL);

      /* Wattch -- Dcache2 access */
      dcache2_access++;

      if (cmd == Read)
	return lat;
      else
	{
	  /* FIXME: unlimited write buffers */
	  return 0;
	}
    }
  else
    {
      /* access main memory */
      if (cmd == Read)
	return mem_access_latency(bsize);
      else
	{
	  /* FIXME: unlimited write buffers */
	  return 0;
	}
    }
}

/* l2 data cache block miss handler function */
static unsigned int			/* latency of block access */
dl2_access_fn(enum mem_cmd cmd,		/* access cmd, Read or Write */
	      md_addr_t baddr,		/* block address to access */
	      int bsize,		/* size of block to access */
	      struct cache_blk_t *blk,	/* ptr to block in upper level */
	      tick_t now,		/* time of access */
	      int context_id)           /* context id */
{

  /* Wattch -- main memory access -- Wattch-FIXME (offchip) */

  /* this is a miss to the lowest level, so access main memory */
  if (cmd == Read)
    return mem_access_latency(bsize);
  else
    {
      /* FIXME: unlimited write buffers */
      return 0;
    }
}

/* l1 inst cache l1 block miss handler function */
static unsigned int			/* latency of block access */
il1_access_fn(enum mem_cmd cmd,		/* access cmd, Read or Write */
	      md_addr_t baddr,		/* block address to access */
	      int bsize,		/* size of block to access */
	      struct cache_blk_t *blk,	/* ptr to block in upper level */
	      tick_t now,		/* time of access */
	      int context_id)           /* context_id of the access */
{
  unsigned int lat;

if (cache_il2)
    {
      /* access next level of inst cache hierarchy */
      lat = cache_access(cache_il2, cmd, baddr, context_id, NULL, bsize,
			 /* now */now, /* pudata */NULL, /* repl addr */NULL);

      /* Wattch -- Dcache2 access */
      dcache2_access++;

      if (cmd == Read)
	return lat;
      else
	panic("writes to instruction memory not supported");
    }
  else
    {
      /* access main memory */
      if (cmd == Read)
	return mem_access_latency(bsize);
      else
	panic("writes to instruction memory not supported");
    }
}

/* l2 inst cache block miss handler function */
static unsigned int			/* latency of block access */
il2_access_fn(enum mem_cmd cmd,		/* access cmd, Read or Write */
	      md_addr_t baddr,		/* block address to access */
	      int bsize,		/* size of block to access */
	      struct cache_blk_t *blk,	/* ptr to block in upper level */
	      tick_t now,		/* time of access */
	      int context_id)
{
  /* Wattch -- main memory access -- Wattch-FIXME (offchip) */

  /* this is a miss to the lowest level, so access main memory */
  if (cmd == Read)
    return mem_access_latency(bsize);
  else
    panic("writes to instruction memory not supported");
}


/*
 * TLB miss handlers
 */

/* inst cache block miss handler function */
static unsigned int			/* latency of block access */
itlb_access_fn(enum mem_cmd cmd,	/* access cmd, Read or Write */
	       md_addr_t baddr,		/* block address to access */
	       int bsize,		/* size of block to access */
	       struct cache_blk_t *blk,	/* ptr to block in upper level */
	       tick_t now,		/* time of access */
	       int context_id)
{
  md_addr_t *phy_page_ptr = (md_addr_t *)blk->user_data;

  /* no real memory access, however, should have user data space attached */
  assert(phy_page_ptr);

  /* fake translation, for now... */
  *phy_page_ptr = 0;

  /* return tlb miss latency */
  return tlb_miss_lat;
}

/* data cache block miss handler function */
static unsigned int			/* latency of block access */
dtlb_access_fn(enum mem_cmd cmd,	/* access cmd, Read or Write */
	       md_addr_t baddr,	/* block address to access */
	       int bsize,		/* size of block to access */
	       struct cache_blk_t *blk,	/* ptr to block in upper level */
	       tick_t now,		/* time of access */
	       int context_id)
{
  md_addr_t *phy_page_ptr = (md_addr_t *)blk->user_data;

  /* no real memory access, however, should have user data space attached */
  assert(phy_page_ptr);

  /* fake translation, for now... */
  *phy_page_ptr = 0;

  /* return tlb miss latency */
  return tlb_miss_lat;
}


/* register simulator-specific options */
void
sim_reg_options(struct opt_odb_t *odb)
{
  opt_reg_header(odb, 
"sim-outorder: This simulator implements a very detailed out-of-order issue\n"
"superscalar processor with a two-level memory system and speculative\n"
"execution support.  This simulator is a performance simulator, tracking the\n"
"latency of all pipeline operations.\n"
		 );

  /* instruction limit */

  opt_reg_uint(odb, "-max:inst", "maximum number of inst's to execute",
	       &max_insts, /* default */1000000,
	       /* print */TRUE, /* format */NULL);

  /* trace options */

  opt_reg_int(odb, "-fastfwd", "number of insts skipped before timing starts",
	      &fastfwd_count, /* default */1000000,
	      /* print */TRUE, /* format */NULL);
  opt_reg_string_list(odb, "-ptrace",
	      "generate pipetrace, i.e., <fname|stdout|stderr> <range>",
	      ptrace_opts, /* arr_sz */2, &ptrace_nelt, /* default */NULL,
	      /* !print */FALSE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_note(odb,
"  Pipetrace range arguments are formatted as follows:\n"
"\n"
"    {{@|#}<start>}:{{@|#|+}<end>}\n"
"\n"
"  Both ends of the range are optional, if neither are specified, the entire\n"
"  execution is traced.  Ranges that start with a `@' designate an address\n"
"  range to be traced, those that start with an `#' designate a cycle count\n"
"  range.  All other range values represent an instruction count range.  The\n"
"  second argument, if specified with a `+', indicates a value relative\n"
"  to the first argument, e.g., 1000:+100 == 1000:1100.  Program symbols may\n"
"  be used in all contexts.\n"
"\n"
"    Examples:   -ptrace FOO.trc #0:#1000\n"
"                -ptrace BAR.trc @2000:\n"
"                -ptrace BLAH.trc :1500\n"
"                -ptrace UXXE.trc :\n"
"                -ptrace FOOBAR.trc @main:+278\n"
	       );

  /* ifetch options */

  opt_reg_int(odb, "-fetch:speed",
	      "speed of front-end of machine relative to execution core",
	      &fetch_speed, /* default */1,
	      /* print */TRUE, /* format */NULL);

  /* branch predictor options */

  opt_reg_note(odb,
"  Branch predictor configuration examples for 2-level predictor:\n"
"    Configurations:   N, M, W, X\n"
"      N   # entries in first level (# of shift register(s))\n"
"      W   width of shift register(s)\n"
"      M   # entries in 2nd level (# of counters, or other FSM)\n"
"      X   (yes-1/no-0) xor history and address for 2nd level index\n"
"    Sample predictors:\n"
"      GAg     : 1, W, 2^W, 0\n"
"      GAp     : 1, W, M (M > 2^W), 0\n"
"      PAg     : N, W, 2^W, 0\n"
"      PAp     : N, W, M (M == 2^(N+W)), 0\n"
"      gshare  : 1, W, 2^W, 1\n"
"  Predictor `comb' combines a bimodal and a 2-level predictor.\n"
               );

  opt_reg_string(odb, "-bpred",
		 "branch predictor type {nottaken|taken|perfect|bimod|2lev|comb}",
                 &pred_type, /* default */"bimod",
                 /* print */TRUE, /* format */NULL);

  opt_reg_int_list(odb, "-bpred:bimod",
		   "bimodal predictor config (<table size>)",
		   bimod_config, bimod_nelt, &bimod_nelt,
		   /* default */bimod_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int_list(odb, "-bpred:2lev",
                   "2-level predictor config "
		   "(<l1size> <l2size> <hist_size> <xor>)",
                   twolev_config, twolev_nelt, &twolev_nelt,
		   /* default */twolev_config,
                   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int_list(odb, "-bpred:comb",
		   "combining predictor config (<meta_table_size>)",
		   comb_config, comb_nelt, &comb_nelt,
		   /* default */comb_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int(odb, "-bpred:ras",
              "return address stack size (0 for no return stack)",
              &ras_size, /* default */ras_size,
              /* print */TRUE, /* format */NULL);

  opt_reg_int_list(odb, "-bpred:btb",
		   "BTB config (<num_sets> <associativity>)",
		   btb_config, btb_nelt, &btb_nelt,
		   /* default */btb_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_string(odb, "-bpred:spec_update",
		 "speculative predictors update in {ID|WB} (default non-spec)",
		 &bpred_spec_opt, /* default */NULL,
		 /* print */TRUE, /* format */NULL);



  /****************** LOAD-LATENCY PREDICTOR **************************/
  opt_reg_string(odb, "-cpred",
		 "cache load-latency predictor type {nottaken|taken|perfect|bimod|2lev|comb}",
                 &cpred_type, /* default */"bimod",
                 /* print */TRUE, /* format */NULL);

  opt_reg_int_list(odb, "-cpred:bimod",
		   "cache load-latency bimodal predictor config (<table size>)",
		   cbimod_config, cbimod_nelt, &cbimod_nelt,
		   /* default */cbimod_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int_list(odb, "-cpred:2lev",
                   "cache load-latency 2-level predictor config "
		   "(<l1size> <l2size> <hist_size> <xor>)",
                   ctwolev_config, ctwolev_nelt, &ctwolev_nelt,
		   /* default */ctwolev_config,
                   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int_list(odb, "-cpred:comb",
		   "cache load-latency combining predictor config (<meta_table_size>)",
		   ccomb_config, ccomb_nelt, &ccomb_nelt,
		   /* default */ccomb_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int(odb, "-cpred:ras",
              "return address stack size (0 for no return stack)",
              &cras_size, /* default */cras_size,
              /* print */TRUE, /* format */NULL);

  opt_reg_int_list(odb, "-cpred:btb",
		   "cache load-latency BTB config (<num_sets> <associativity>)",
		   cbtb_config, cbtb_nelt, &cbtb_nelt,
		   /* default */cbtb_config,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);
  /****************** LOAD-LATENCY PREDICTOR **************************/

  /* decode options */

  opt_reg_int(odb, "-decode:width",
	      "instruction decode B/W (insts/cycle)",
	      &decode_width, /* default */4,
	      /* print */TRUE, /* format */NULL);

  /* issue options */

  opt_reg_int(odb, "-issue:width",
	      "instruction issue B/W (insts/cycle)",
	      &issue_width, /* default */4,
	      /* print */TRUE, /* format */NULL);

  opt_reg_flag(odb, "-issue:inorder", "run pipeline with in-order issue",
	       &inorder_issue, /* default */FALSE,
	       /* print */TRUE, /* format */NULL);

  opt_reg_flag(odb, "-issue:wrongpath",
	       "issue instructions down wrong execution paths",
	       &include_spec, /* default */TRUE,
	       /* print */TRUE, /* format */NULL);

  /* commit options */

  opt_reg_int(odb, "-commit:width",
	      "instruction commit B/W (insts/cycle)",
	      &commit_width, /* default */4,
	      /* print */TRUE, /* format */NULL);

  /* core structure options */

  opt_reg_int(odb, "-rob:size",
	      "reorder buffer (ROB) size",
	      &ROB_size, /* default */128,
	      /* print */TRUE, /* format */NULL);

  opt_reg_string(odb, "-fetch:policy |icount|round_robin|",
	      "fetch policy",
	      &fetch_policy, /* default */"icount",
	      /* print */TRUE, /* format */NULL);

  opt_reg_string(odb, "-recovery:model",
	      "Alpha squash recovery or perfect predition: |squash|perfect|",
	      &recovery_model, /* default */"squash",
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-iq:size",
	      "issue queue (IQ) size",
	      &iq_size, /* default */32,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-iq:issue_exec_delay",
	      "minimum cycles between issue and execution",
	      &ISSUE_EXEC_DELAY, /* default */1,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-fetch_rename_delay",
	      "number of cycles between fetch and rename stages",
	      &FETCH_RENAME_DELAY, /* default */4,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-rename_dispatch_delay",
	      "number cycles between rename and dispatch stages",
	      &RENAME_DISPATCH_DELAY, /* default */1,
	      /* print */TRUE, /* format */NULL);
  
  opt_reg_int(odb, "-rf:size",
	      "register file (RF) size for each the INT and FP physical register file)",
	      &rf_size, /* default */128,
	      /* print */TRUE, /* format */NULL);
  
  /* memory scheduler options  */

  opt_reg_int(odb, "-lsq:size",
	      "load/store queue (LSQ) size",
	      &LSQ_size, /* default */48,
	      /* print */TRUE, /* format */NULL);

  /* cache options */

  opt_reg_string(odb, "-cache:dl1",
		 "l1 data cache config, i.e., {<config>|none}",
		 &cache_dl1_opt, "dl1:256:32:4:l",
		 /* print */TRUE, NULL);

  opt_reg_note(odb,
"  The cache config parameter <config> has the following format:\n"
"\n"
"    <name>:<nsets>:<bsize>:<assoc>:<repl>\n"
"\n"
"    <name>   - name of the cache being defined\n"
"    <nsets>  - number of sets in the cache\n"
"    <bsize>  - block size of the cache\n"
"    <assoc>  - associativity of the cache\n"
"    <repl>   - block replacement strategy, 'l'-LRU, 'f'-FIFO, 'r'-random\n"
"\n"
"    Examples:   -cache:dl1 dl1:4096:32:1:l\n"
"                -dtlb dtlb:128:4096:32:r\n"
	       );

  opt_reg_int(odb, "-cache:dl1lat",
	      "l1 data cache hit latency (in cycles)",
	      &cache_dl1_lat, /* default */1,
	      /* print */TRUE, /* format */NULL);

  opt_reg_string(odb, "-cache:dl2",
		 "l2 data cache config, i.e., {<config>|none}",
		 &cache_dl2_opt, "ul2:1024:64:8:l",
		 /* print */TRUE, NULL);

  opt_reg_int(odb, "-cache:dl2lat",
	      "l2 data cache hit latency (in cycles)",
	      &cache_dl2_lat, /* default */6,
	      /* print */TRUE, /* format */NULL);

  opt_reg_string(odb, "-cache:il1",
		 "l1 inst cache config, i.e., {<config>|dl1|dl2|none}",
		 &cache_il1_opt, "il1:512:32:2:l",
		 /* print */TRUE, NULL);

  opt_reg_note(odb,
"  Cache levels can be unified by pointing a level of the instruction cache\n"
"  hierarchy at the data cache hiearchy using the \"dl1\" and \"dl2\" cache\n"
"  configuration arguments.  Most sensible combinations are supported, e.g.,\n"
"\n"
"    A unified l2 cache (il2 is pointed at dl2):\n"
"      -cache:il1 il1:128:64:1:l -cache:il2 dl2\n"
"      -cache:dl1 dl1:256:32:1:l -cache:dl2 ul2:1024:64:2:l\n"
"\n"
"    Or, a fully unified cache hierarchy (il1 pointed at dl1):\n"
"      -cache:il1 dl1\n"
"      -cache:dl1 ul1:256:32:1:l -cache:dl2 ul2:1024:64:2:l\n"
	       );

  opt_reg_int(odb, "-cache:il1lat",
	      "l1 instruction cache hit latency (in cycles)",
	      &cache_il1_lat, /* default */1,
	      /* print */TRUE, /* format */NULL);

  opt_reg_string(odb, "-cache:il2",
		 "l2 instruction cache config, i.e., {<config>|dl2|none}",
		 &cache_il2_opt, "dl2",
		 /* print */TRUE, NULL);

  opt_reg_int(odb, "-cache:il2lat",
	      "l2 instruction cache hit latency (in cycles)",
	      &cache_il2_lat, /* default */6,
	      /* print */TRUE, /* format */NULL);

  opt_reg_flag(odb, "-cache:flush", "flush caches on system calls",
	       &flush_on_syscalls, /* default */FALSE, /* print */TRUE, NULL);

  opt_reg_flag(odb, "-cache:icompress",
	       "convert 64-bit inst addresses to 32-bit inst equivalents",
	       &compress_icache_addrs, /* default */FALSE,
	       /* print */TRUE, NULL);

  /* mem options */
  opt_reg_int_list(odb, "-mem:lat",
		   "memory access latency (<first_chunk> <inter_chunk>)",
		   mem_lat, mem_nelt, &mem_nelt, mem_lat,
		   /* print */TRUE, /* format */NULL, /* !accrue */FALSE);

  opt_reg_int(odb, "-mem:width", "memory access bus width (in bytes)",
	      &mem_bus_width, /* default */8,
	      /* print */TRUE, /* format */NULL);

  /* TLB options */

  opt_reg_string(odb, "-tlb:itlb",
		 "instruction TLB config, i.e., {<config>|none}",
		 &itlb_opt, "itlb:16:4096:4:l", /* print */TRUE, NULL);

  opt_reg_string(odb, "-tlb:dtlb",
		 "data TLB config, i.e., {<config>|none}",
		 &dtlb_opt, "dtlb:32:4096:4:l", /* print */TRUE, NULL);

  opt_reg_int(odb, "-tlb:lat",
	      "inst/data TLB miss latency (in cycles)",
	      &tlb_miss_lat, /* default */30,
	      /* print */TRUE, /* format */NULL);

  /* resource configuration */

  opt_reg_int(odb, "-res:ialu",
	      "total number of integer ALU's available",
	      &res_ialu, /* default */fu_config[FU_IALU_INDEX].quantity,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-res:imult",
	      "total number of integer multiplier/dividers available",
	      &res_imult, /* default */fu_config[FU_IMULT_INDEX].quantity,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-res:memport",
	      "total number of memory system ports available (to CPU)",
	      &res_memport, /* default */fu_config[FU_MEMPORT_INDEX].quantity,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-res:fpalu",
	      "total number of floating point ALU's available",
	      &res_fpalu, /* default */fu_config[FU_FPALU_INDEX].quantity,
	      /* print */TRUE, /* format */NULL);

  opt_reg_int(odb, "-res:fpmult",
	      "total number of floating point multiplier/dividers available",
	      &res_fpmult, /* default */fu_config[FU_FPMULT_INDEX].quantity,
	      /* print */TRUE, /* format */NULL);

  opt_reg_string_list(odb, "-pcstat",
		      "profile stat(s) against text addr's (mult uses ok)",
		      pcstat_vars, MAX_PCSTAT_VARS, &pcstat_nelt, NULL,
		      /* !print */FALSE, /* format */NULL, /* accrue */TRUE);

  opt_reg_flag(odb, "-power:print_stats",
	      "print power statistics collected by wattch?",
	      &print_power_stats, /* default */TRUE,
	      /* print */TRUE, /* format */NULL);

}

/* check simulator-specific option values */
void
sim_check_options(struct opt_odb_t *odb,        /* options database */
		  int argc, char **argv)        /* command line arguments */
{
  char name[128], c;
  int nsets, bsize, assoc;
  int i;

  if (fastfwd_count < 0 || fastfwd_count >= 2147483647)
    fatal("bad fast forward count: %d", fastfwd_count);

  ifq_size = FETCH_RENAME_DELAY * decode_width;

  if (fetch_speed < 1)
    fatal("front-end speed must be positive and non-zero");

  for(i = 0; i < MAX_CONTEXTS;i++)
    {
      if (!mystricmp(pred_type, "perfect"))
	{
	  /* perfect predictor */
	  contexts[i].pred = NULL;
	  pred_perfect = TRUE;
	}
      else if (!mystricmp(pred_type, "taken"))
	{
	  /* static predictor, not taken */
	  contexts[i].pred = bpred_create(BPredTaken, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	}
      else if (!mystricmp(pred_type, "nottaken"))
	{
	  /* static predictor, taken */
	  contexts[i].pred = bpred_create(BPredNotTaken, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	}
      else if (!mystricmp(pred_type, "bimod"))
	{
	  /* bimodal predictor, bpred_create() checks BTB_SIZE */
	  if (bimod_nelt != 1)
	    fatal("bad bimod predictor config (<table_size>)");
	  if (btb_nelt != 2)
	    fatal("bad btb config (<num_sets> <associativity>)");

	  /* bimodal predictor, bpred_create() checks BTB_SIZE */
	  contexts[i].pred = bpred_create(BPred2bit,
			      /* bimod table size */bimod_config[0],
			      /* 2lev l1 size */0,
			      /* 2lev l2 size */0,
			      /* meta table size */0,
			      /* history reg size */0,
			      /* history xor address */0,
			      /* btb sets */btb_config[0],
			      /* btb assoc */btb_config[1],
			      /* ret-addr stack size */ras_size);
	}
      else if (!mystricmp(pred_type, "2lev"))
	{
	  /* 2-level adaptive predictor, bpred_create() checks args */
	  if (twolev_nelt != 4)
	    fatal("bad 2-level pred config (<l1size> <l2size> <hist_size> <xor>)");
	  if (btb_nelt != 2)
	    fatal("bad btb config (<num_sets> <associativity>)");

	  contexts[i].pred = bpred_create(BPred2Level,
			      /* bimod table size */0,
			      /* 2lev l1 size */twolev_config[0],
			      /* 2lev l2 size */twolev_config[1],
			      /* meta table size */0,
			      /* history reg size */twolev_config[2],
			      /* history xor address */twolev_config[3],
			      /* btb sets */btb_config[0],
			      /* btb assoc */btb_config[1],
			      /* ret-addr stack size */ras_size);
	}
      else if (!mystricmp(pred_type, "comb"))
	{
	  /* combining predictor, bpred_create() checks args */
	  if (twolev_nelt != 4)
	    fatal("bad 2-level pred config (<l1size> <l2size> <hist_size> <xor>)");
	  if (bimod_nelt != 1)
	    fatal("bad bimod predictor config (<table_size>)");
	  if (comb_nelt != 1)
	    fatal("bad combining predictor config (<meta_table_size>)");
	  if (btb_nelt != 2)
	    fatal("bad btb config (<num_sets> <associativity>)");

	  contexts[i].pred = bpred_create(BPredComb,
			      /* bimod table size */bimod_config[0],
			      /* l1 size */twolev_config[0],
			      /* l2 size */twolev_config[1],
			      /* meta table size */comb_config[0],
			      /* history reg size */twolev_config[2],
			      /* history xor address */twolev_config[3],
			      /* btb sets */btb_config[0],
			      /* btb assoc */btb_config[1],
			      /* ret-addr stack size */ras_size);
	}
      else
	fatal("cannot parse predictor type `%s'", pred_type);


      /**************************** load hit/miss predictor ***********************/
      /* bimodal predictor, bpred_create() checks BTB_SIZE */
       if (!mystricmp(cpred_type, "perfect"))
	{
	  /* perfect predictor */
	  contexts[i].load_lat_pred = NULL;
	  pred_perfect = TRUE;
	}
      else if (!mystricmp(cpred_type, "taken"))
	{
	  /* static predictor, not taken */
	  contexts[i].load_lat_pred = bpred_create(BPredTaken, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	}
      else if (!mystricmp(cpred_type, "nottaken"))
	{
	  /* static predictor, taken */
	  contexts[i].load_lat_pred = bpred_create(BPredNotTaken, 0, 0, 0, 0, 0, 0, 0, 0, 0);
	}
      else if (!mystricmp(cpred_type, "bimod"))
	{
	  /* bimodal predictor, bpred_create() checks BTB_SIZE */
	  if (cbimod_nelt != 1)
	    fatal("bad cbimod predictor config (<table_size>)");
	  if (cbtb_nelt != 2)
	    fatal("bad cbtb config (<num_sets> <associativity>)");

	  /* bimodal predictor, bpred_create() checks BTB_SIZE */
	  contexts[i].load_lat_pred = bpred_create(BPred2bit,
			      /* bimod table size */cbimod_config[0],
			      /* 2lev l1 size */0,
			      /* 2lev l2 size */0,
			      /* meta table size */0,
			      /* history reg size */0,
			      /* history xor address */0,
			      /* btb sets */cbtb_config[0],
			      /* btb assoc */cbtb_config[1],
			      /* ret-addr stack size */cras_size);
	}
      else if (!mystricmp(cpred_type, "2lev"))
	{
	  /* 2-level adaptive predictor, bpred_create() checks args */
	  if (ctwolev_nelt != 4)
	    fatal("bad 2-level pred config (<l1size> <l2size> <hist_size> <xor>)");
	  if (cbtb_nelt != 2)
	    fatal("bad btb config (<num_sets> <associativity>)");

	  contexts[i].load_lat_pred = bpred_create(BPred2Level,
			      /* bimod table size */0,
			      /* 2lev l1 size */ctwolev_config[0],
			      /* 2lev l2 size */ctwolev_config[1],
			      /* meta table size */0,
			      /* history reg size */ctwolev_config[2],
			      /* history xor address */ctwolev_config[3],
			      /* btb sets */cbtb_config[0],
			      /* btb assoc */cbtb_config[1],
			      /* ret-addr stack size */cras_size);
	}
      else if (!mystricmp(cpred_type, "comb"))
	{
	  /* combining predictor, bpred_create() checks args */
	  if (ctwolev_nelt != 4)
	    fatal("bad 2-level pred config (<l1size> <l2size> <hist_size> <xor>)");
	  if (cbimod_nelt != 1)
	    fatal("bad bimod predictor config (<table_size>)");
	  if (ccomb_nelt != 1)
	    fatal("bad combining predictor config (<meta_table_size>)");
	  if (cbtb_nelt != 2)
	    fatal("bad btb config (<num_sets> <associativity>)");

	  contexts[i].load_lat_pred = bpred_create(BPredComb,
			      /* bimod table size */cbimod_config[0],
			      /* l1 size */ctwolev_config[0],
			      /* l2 size */ctwolev_config[1],
			      /* meta table size */ccomb_config[0],
			      /* history reg size */ctwolev_config[2],
			      /* history xor address */ctwolev_config[3],
			      /* btb sets */cbtb_config[0],
			      /* btb assoc */cbtb_config[1],
			      /* ret-addr stack size */cras_size);
	}
      else
	fatal("cannot parse load-latency predictor type `%s'", pred_type);
    }


  if (!bpred_spec_opt)
    bpred_spec_update = spec_CT;
  else if (!mystricmp(bpred_spec_opt, "ID"))
    bpred_spec_update = spec_ID;
  else if (!mystricmp(bpred_spec_opt, "WB"))
    bpred_spec_update = spec_WB;
  else
    fatal("bad speculative update stage specifier, use {ID|WB}");

  if (decode_width < 1 || (decode_width & (decode_width-1)) != 0)
    fatal("issue width must be positive non-zero and a power of two");

  if (issue_width < 1 || (issue_width & (issue_width-1)) != 0)
    fatal("issue width must be positive non-zero and a power of two");

  if (commit_width < 1)
    fatal("commit width must be positive non-zero");

  if (ROB_size < 2)
    fatal("ROB size must be a positive number > 1");

  if (LSQ_size < 2)
    fatal("LSQ size must be a positive number > 1");

  if (iq_size < 2)
    fatal("IQ size must be a positive number > 1");

  if (rf_size < 2)
    fatal("RF size must be a positive number > 1");
  
  /*
   * Each thread requires 32 integer and 32 floating point registers for
   * the architectural state
   */
  if (rf_size < (32 * num_contexts + 1))
    fatal("Must have enough registers for arch state for each thread!!");
  
  /* use a level 1 D-cache? */
  if (!mystricmp(cache_dl1_opt, "none"))
    {
      cache_dl1 = NULL;

      /* the level 2 D-cache cannot be defined */
      if (strcmp(cache_dl2_opt, "none"))
	fatal("the l1 data cache must defined if the l2 cache is defined");
      cache_dl2 = NULL;
    }
  else /* dl1 is defined */
    {
      if (sscanf(cache_dl1_opt, "%[^:]:%d:%d:%d:%c",
		 name, &nsets, &bsize, &assoc, &c) != 5)
	fatal("bad l1 D-cache parms: <name>:<nsets>:<bsize>:<assoc>:<repl>");
      cache_dl1 = cache_create(name, nsets, bsize, /* balloc */FALSE,
			       /* usize */0, assoc, cache_char2policy(c),
			       dl1_access_fn, /* hit lat */cache_dl1_lat);

      /* is the level 2 D-cache defined? */
      if (!mystricmp(cache_dl2_opt, "none"))
	cache_dl2 = NULL;
      else
	{
	  if (sscanf(cache_dl2_opt, "%[^:]:%d:%d:%d:%c",
		     name, &nsets, &bsize, &assoc, &c) != 5)
	    fatal("bad l2 D-cache parms: "
		  "<name>:<nsets>:<bsize>:<assoc>:<repl>");
	  cache_dl2 = cache_create(name, nsets, bsize, /* balloc */FALSE,
				   /* usize */0, assoc, cache_char2policy(c),
				   dl2_access_fn, /* hit lat */cache_dl2_lat);
	}
    }

  /* use a level 1 I-cache? */
  if (!mystricmp(cache_il1_opt, "none"))
    {
      cache_il1 = NULL;

      /* the level 2 I-cache cannot be defined */
      if (strcmp(cache_il2_opt, "none"))
	fatal("the l1 inst cache must defined if the l2 cache is defined");
      cache_il2 = NULL;
    }
  else if (!mystricmp(cache_il1_opt, "dl1"))
    {
      if (!cache_dl1)
	fatal("I-cache l1 cannot access D-cache l1 as it's undefined");
      cache_il1 = cache_dl1;

      /* the level 2 I-cache cannot be defined */
      if (strcmp(cache_il2_opt, "none"))
	fatal("the l1 inst cache must defined if the l2 cache is defined");
      cache_il2 = NULL;
    }
  else if (!mystricmp(cache_il1_opt, "dl2"))
    {
      if (!cache_dl2)
	fatal("I-cache l1 cannot access D-cache l2 as it's undefined");
      cache_il1 = cache_dl2;

      /* the level 2 I-cache cannot be defined */
      if (strcmp(cache_il2_opt, "none"))
	fatal("the l1 inst cache must defined if the l2 cache is defined");
      cache_il2 = NULL;
    }
  else /* il1 is defined */
    {
      if (sscanf(cache_il1_opt, "%[^:]:%d:%d:%d:%c",
		 name, &nsets, &bsize, &assoc, &c) != 5)
	fatal("bad l1 I-cache parms: <name>:<nsets>:<bsize>:<assoc>:<repl>");
      cache_il1 = cache_create(name, nsets, bsize, /* balloc */FALSE,
			       /* usize */0, assoc, cache_char2policy(c),
			       il1_access_fn, /* hit lat */cache_il1_lat);

      /* is the level 2 D-cache defined? */
      if (!mystricmp(cache_il2_opt, "none"))
	cache_il2 = NULL;
      else if (!mystricmp(cache_il2_opt, "dl2"))
	{
	  if (!cache_dl2)
	    fatal("I-cache l2 cannot access D-cache l2 as it's undefined");
	  cache_il2 = cache_dl2;
	}
      else
	{
	  if (sscanf(cache_il2_opt, "%[^:]:%d:%d:%d:%c",
		     name, &nsets, &bsize, &assoc, &c) != 5)
	    fatal("bad l2 I-cache parms: "
		  "<name>:<nsets>:<bsize>:<assoc>:<repl>");
	  cache_il2 = cache_create(name, nsets, bsize, /* balloc */FALSE,
				   /* usize */0, assoc, cache_char2policy(c),
				   il2_access_fn, /* hit lat */cache_il2_lat);
	}
    }

  /* use an I-TLB? */
  if (!mystricmp(itlb_opt, "none"))
    itlb = NULL;
  else
    {
      if (sscanf(itlb_opt, "%[^:]:%d:%d:%d:%c",
		 name, &nsets, &bsize, &assoc, &c) != 5)
	fatal("bad TLB parms: <name>:<nsets>:<page_size>:<assoc>:<repl>");
      itlb = cache_create(name, nsets, bsize, /* balloc */FALSE,
			  /* usize */sizeof(md_addr_t), assoc,
			  cache_char2policy(c), itlb_access_fn,
			  /* hit latency */1);
    }

  /* use a D-TLB? */
  if (!mystricmp(dtlb_opt, "none"))
    dtlb = NULL;
  else
    {
      if (sscanf(dtlb_opt, "%[^:]:%d:%d:%d:%c",
		 name, &nsets, &bsize, &assoc, &c) != 5)
	fatal("bad TLB parms: <name>:<nsets>:<page_size>:<assoc>:<repl>");
      dtlb = cache_create(name, nsets, bsize, /* balloc */FALSE,
			  /* usize */sizeof(md_addr_t), assoc,
			  cache_char2policy(c), dtlb_access_fn,
			  /* hit latency */1);
    }

  if (cache_dl1_lat < 1)
    fatal("l1 data cache latency must be greater than zero");

  if (cache_dl2_lat < 1)
    fatal("l2 data cache latency must be greater than zero");

  if (cache_il1_lat < 1)
    fatal("l1 instruction cache latency must be greater than zero");

  if (cache_il2_lat < 1)
    fatal("l2 instruction cache latency must be greater than zero");

  if (mem_nelt != 2)
    fatal("bad memory access latency (<first_chunk> <inter_chunk>)");

  if (mem_lat[0] < 1 || mem_lat[1] < 1)
    fatal("all memory access latencies must be greater than zero");

  if (mem_bus_width < 1 || (mem_bus_width & (mem_bus_width-1)) != 0)
    fatal("memory bus width must be positive non-zero and a power of two");

  if (tlb_miss_lat < 1)
    fatal("TLB miss latency must be greater than zero");

  if (res_ialu < 1)
    fatal("number of integer ALU's must be greater than zero");
  if (res_ialu > MAX_INSTS_PER_CLASS)
    fatal("number of integer ALU's must be <= MAX_INSTS_PER_CLASS");
  fu_config[FU_IALU_INDEX].quantity = res_ialu;
  
  if (res_imult < 1)
    fatal("number of integer multiplier/dividers must be greater than zero");
  if (res_imult > MAX_INSTS_PER_CLASS)
    fatal("number of integer mult/div's must be <= MAX_INSTS_PER_CLASS");
  fu_config[FU_IMULT_INDEX].quantity = res_imult;
  
  if (res_memport < 1)
    fatal("number of memory system ports must be greater than zero");
  if (res_memport > MAX_INSTS_PER_CLASS)
    fatal("number of memory system ports must be <= MAX_INSTS_PER_CLASS");
  fu_config[FU_MEMPORT_INDEX].quantity = res_memport;
  
  if (res_fpalu < 1)
    fatal("number of floating point ALU's must be greater than zero");
  if (res_fpalu > MAX_INSTS_PER_CLASS)
    fatal("number of floating point ALU's must be <= MAX_INSTS_PER_CLASS");
  fu_config[FU_FPALU_INDEX].quantity = res_fpalu;
  
  if (res_fpmult < 1)
    fatal("number of floating point multiplier/dividers must be > zero");
  if (res_fpmult > MAX_INSTS_PER_CLASS)
    fatal("number of FP mult/div's must be <= MAX_INSTS_PER_CLASS");
  fu_config[FU_FPMULT_INDEX].quantity = res_fpmult;
}

/* print simulator-specific configuration information */
void
sim_aux_config(FILE *stream)            /* output stream */
{
  /* nada */
}

/* register simulator-specific statistics */
void
sim_reg_stats(struct stat_sdb_t *sdb)   /* stats database */
{
  int i;
  stat_reg_counter(sdb, "sim_num_insn",
		   "total number of instructions committed",
		   &sim_num_insn, sim_num_insn, NULL);
  for(i=0;i<num_contexts;i++){
    char str[20];
    sprintf(str,"sim_num_insn %d", i);
    stat_reg_counter(sdb, str,
		     "total number of instructions committed for this thread",
		     &contexts[i].sim_num_insn, contexts[i].sim_num_insn, NULL);
  }
  stat_reg_counter(sdb, "sim_num_refs",
		   "total number of loads and stores committed",
		   &sim_num_refs, 0, NULL);
  stat_reg_counter(sdb, "sim_num_loads",
		   "total number of loads committed",
		   &sim_num_loads, 0, NULL);
  stat_reg_formula(sdb, "sim_num_stores",
		   "total number of stores committed",
		   "sim_num_refs - sim_num_loads", NULL);
  stat_reg_counter(sdb, "sim_num_branches",
		   "total number of branches committed",
		   &sim_num_branches, /* initial value */0, /* format */NULL);
  stat_reg_int(sdb, "sim_elapsed_time",
	       "total simulation time in seconds",
	       &sim_elapsed_time, 0, NULL);
  stat_reg_formula(sdb, "sim_inst_rate",
		   "simulation speed (in insts/sec)",
		   "sim_num_insn / sim_elapsed_time", NULL);

  stat_reg_counter(sdb, "sim_total_insn",
		   "total number of instructions executed",
		   &sim_total_insn, 0, NULL);
  stat_reg_counter(sdb, "sim_total_refs",
		   "total number of loads and stores executed",
		   &sim_total_refs, 0, NULL);
  stat_reg_counter(sdb, "sim_total_loads",
		   "total number of loads executed",
		   &sim_total_loads, 0, NULL);
  stat_reg_formula(sdb, "sim_total_stores",
		   "total number of stores executed",
		   "sim_total_refs - sim_total_loads", NULL);
  stat_reg_counter(sdb, "sim_total_branches",
		   "total number of branches executed",
		   &sim_total_branches, /* initial value */0, /* format */NULL);

  /* register performance stats */
  stat_reg_counter(sdb, "sim_cycle",
		   "total simulation time in cycles",
		   &sim_cycle, /* initial value */0, /* format */NULL);
  stat_reg_formula(sdb, "sim_IPC",
		   "instructions per cycle",
		   "sim_num_insn / sim_cycle", /* format */NULL);
  stat_reg_formula(sdb, "sim_CPI",
		   "cycles per instruction",
		   "sim_cycle / sim_num_insn", /* format */NULL);
  stat_reg_formula(sdb, "sim_exec_BW",
		   "total instructions (mis-spec + committed) per cycle",
		   "sim_total_insn / sim_cycle", /* format */NULL);
  stat_reg_formula(sdb, "sim_IPB",
		   "instruction per branch",
		   "sim_num_insn / sim_num_branches", /* format */NULL);

  stat_reg_counter(sdb, "sim_slip",
                   "total number of slip cycles",
                   &sim_slip, 0, NULL);
  /* register baseline stats */
  stat_reg_formula(sdb, "avg_sim_slip",
                   "the average slip between issue and retirement",
                   "sim_slip / sim_num_insn", NULL);

  /* register predictor stats */
  for(i = 0; i < num_contexts; i++){
    if (contexts[i].pred){
      char name_buf[20], *name;
      
      /* get a name for this predictor */
      switch (contexts[i].pred->class)
	{
	case BPredComb:
	  name = "bpred_comb";
	  break;
	case BPred2Level:
	  name = "bpred_2lev";
	  break;
	case BPred2bit:
	  name = "bpred_bimod";
	  break;
	case BPredTaken:
	  name = "bpred_taken";
	  break;
	case BPredNotTaken:
	  name = "bpred_nottaken";
	  break;
	default:
	  panic("bogus branch predictor class");
	}
      
      sprintf(name_buf, "Thread_%d_%s", i, name);
      name = name_buf;
      
      bpred_reg_stats(contexts[i].pred, sdb, name);
    }
  }

  for(i = 0; i < num_contexts; i++){
    if (contexts[i].load_lat_pred){
      char name_buf[20], *name;
      
      /* get a name for this predictor */
      switch (contexts[i].load_lat_pred->class)
	{
	case BPredComb:
	  name = "cpred_comb";
	  break;
	case BPred2Level:
	  name = "cpred_2lev";
	  break;
	case BPred2bit:
	  name = "cpred_bimod";
	  break;
	case BPredTaken:
	  name = "cpred_taken";
	  break;
	case BPredNotTaken:
	  name = "cpred_nottaken";
	  break;
	default:
	  panic("bogus load-latency predictor class");
	}
      
      sprintf(name_buf, "Thread_%d_%s", i, name);
      name = name_buf;
      
      bpred_reg_stats(contexts[i].load_lat_pred, sdb, name);
    }
  }



  /* register cache stats */
  if (cache_il1
      && (cache_il1 != cache_dl1 && cache_il1 != cache_dl2))
    cache_reg_stats(cache_il1, sdb);
  if (cache_il2
      && (cache_il2 != cache_dl1 && cache_il2 != cache_dl2))
    cache_reg_stats(cache_il2, sdb);
  if (cache_dl1)
    cache_reg_stats(cache_dl1, sdb);
  if (cache_dl2)
    cache_reg_stats(cache_dl2, sdb);
  if (itlb)
    cache_reg_stats(itlb, sdb);
  if (dtlb)
    cache_reg_stats(dtlb, sdb);

  /* debug variable(s) */
  stat_reg_counter(sdb, "sim_invalid_addrs",
		   "total non-speculative bogus addresses seen (debug var)",
                   &sim_invalid_addrs, /* initial value */0, /* format */NULL);

  for (i=0; i<pcstat_nelt; i++)
    {
      char buf[512], buf1[512];
      struct stat_stat_t *stat;

      /* track the named statistical variable by text address */

      /* find it... */
      stat = stat_find_stat(sdb, pcstat_vars[i]);
      if (!stat)
	fatal("cannot locate any statistic named `%s'", pcstat_vars[i]);

      /* stat must be an integral type */
      if (stat->sc != sc_int && stat->sc != sc_uint && stat->sc != sc_counter)
	fatal("`-pcstat' statistical variable `%s' is not an integral type",
	      stat->name);

      /* register this stat */
      pcstat_stats[i] = stat;
      pcstat_lastvals[i] = STATVAL(stat);

      /* declare the sparce text distribution */
      sprintf(buf, "%s_by_pc", stat->name);
      sprintf(buf1, "%s (by text address)", stat->desc);
      pcstat_sdists[i] = stat_reg_sdist(sdb, buf, buf1,
					/* initial value */0,
					/* print format */(PF_COUNT|PF_PDF),
					/* format */"0x%lx %lu %.2f",
					/* print fn */NULL);
    }
  
  for(i=0;i<num_contexts;i++){
    ld_reg_stats(sdb, contexts[i].mem);
    mem_reg_stats(contexts[i].mem, sdb);
  }

  /* register power stats */
  if(print_power_stats){
    power_reg_stats(sdb);
  }

}

/* forward declarations */
static void rob_init(int context_id);
static void lsq_init(int context_id);
static void rslink_init(int nlinks);
static void eventq_init(void);
static void readyq_init(void);
static void waitq_init(void);
static void issue_exec_q_init(void);
static void tracer_init(int context_id);
static void fetch_init(void);

/* initialize the simulator */
void
sim_init(void)
{
  sim_num_refs = 0;

  /* allocate and initialize register file */
  int_reg_file = (struct physreg_t*) calloc(rf_size, sizeof(struct physreg_t));
  fp_reg_file = (struct physreg_t*) calloc(rf_size, sizeof(struct physreg_t));

  /* compute static power estimates */
  calculate_power(&power);
}

/* default register state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_reg_obj(struct regs_t *regs,		/* registers to access */
	      int is_write,			/* access type */
	      enum md_reg_type rt,		/* reg bank to probe */
	      int reg,				/* register number */
	      struct eval_value_t *val); 	/* input, output */
	      

/* default memory state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_mem_obj(struct mem_t *mem,		/* memory space to access */
	      int is_write,			/* access type */
	      md_addr_t addr,			/* address to access */
	      char *p,				/* input/output buffer */
	      int nbytes);			/* size of access */

/* default machine state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_mstate_obj(FILE *stream,			/* output stream */
		 char *cmd,			/* optional command string */
		 struct regs_t *regs,		/* registers to access */
		 struct mem_t *mem);		/* memory space to access */

/* total RS links allocated at program start */
#define MAX_RS_LINKS                   8128

/* load program into simulated state */
void
sim_load_prog(char *fname,		/* program to load */
	      int argc, char **argv,	/* program arguments */
	      char **envp)		/* program environment */
{
  /* load the program into the next available context */
  struct regs_t* regs = &contexts[num_contexts].regs;

  /* initalize the context */
  init_context(&contexts[num_contexts],num_contexts);

  /* load program text and data, set up environment, memory, and regs */
  ld_load_prog(fname, argc, argv, envp, regs, contexts[num_contexts].mem, TRUE);

  /* initalize the PC */
  regs->regs_PC = contexts[num_contexts].mem->ld_prog_entry;
  regs->regs_NPC = contexts[num_contexts].mem->ld_prog_entry + 4;
  /* initalize the context_id */
  regs->context_id = num_contexts;

  /* initialize here, so symbols can be loaded */
  if (ptrace_nelt == 2)
    {
      /* generate a pipeline trace */
      ptrace_open(/* fname */ptrace_opts[0], /* range */ptrace_opts[1], contexts[num_contexts].mem);
    }
  else if (ptrace_nelt == 0)
    {
      /* no pipetracing */;
    }
  else
    fatal("bad pipetrace args, use: <fname|stdout|stderr> <range>");

  /* finish initialization of the simulation engine */
  fu_pool = res_create_pool("fu-pool", fu_config, N_ELT(fu_config));
  rslink_init(MAX_RS_LINKS);
  tracer_init(num_contexts);
  fetch_init();
  eventq_init();
  readyq_init();
  waitq_init();
  issue_exec_q_init();
  /* initalize the ROB for this context */
  rob_init(num_contexts);
  /* initalize the ROB for this context */
  lsq_init(num_contexts);

  /* initialize the DLite debugger */
  dlite_init(simoo_reg_obj, simoo_mem_obj, simoo_mstate_obj, contexts[num_contexts].mem);

  /* incriment the global counter for the number of used contexts */
  num_contexts++;

}

/* dump simulator-specific auxiliary simulator statistics */
void
sim_aux_stats(FILE *stream)             /* output stream */
{
  /* nada */
}

/* un-initialize the simulator */
void
sim_uninit(void)
{
  if (ptrace_nelt > 0)
    ptrace_close();
}


/*
 * processor core definitions and declarations
 */

/* inst tag type, used to tag an operation instance in the ROB */
typedef unsigned int INST_TAG_TYPE;

/* inst sequence type, used to order instructions in the ready list, if
   this rolls over the ready list order temporarily will get messed up,
   but execution will continue and complete correctly */
typedef unsigned int INST_SEQ_TYPE;


/* total input dependencies possible */
/* only 2 of the 3 used by Alpha */
#define MAX_IDEPS               3

/* total output dependencies possible */
/* only 1 of the 2 used by Alpha */
#define MAX_ODEPS               2

/* A re-order buffer (ROB) entry, this record is contained in the program
   order.
   NOTE: the ROB and LSQ share the same structure, this is useful because
   loads and stores are split into two operations: an effective address 
   add and a load/store, the add is inserted into the ROB and the load/store
   inserted into the LSQ, allowing the add to wake up the load/store when
   effective address computation has finished */
struct ROB_entry {
  /* inst info */
  md_inst_t IR;			        /* instruction bits */
  enum md_opcode op;			/* decoded instruction opcode */
  md_addr_t PC, next_PC, pred_PC;	/* inst PC, next PC, predicted PC */
  int in_LSQ;
  int LSQ_index;                           /* non-zero if op is in LSQ */
  int ea_comp;				/* non-zero if op is an addr comp */
  int recover_inst;			/* start of mis-speculation? */
  int stack_recover_idx;		/* non-speculative TOS for RSB pred */
  struct bpred_update_t dir_update;	/* bpred direction update info */
  int spec_mode;			/* non-zero if issued in spec_mode */
  md_addr_t addr;			/* effective address for ld/st's */
  INST_TAG_TYPE tag;			/* ROB slot tag, increment to
					   squash operation */
  INST_SEQ_TYPE seq;			/* instruction sequence, used to
					   sort the ready list and tag inst */
  unsigned int ptrace_seq;		/* pipetrace sequence number */

  /* Wattch: values of source operands and result operand used for AF generation */
  quad_t val_ra, val_rb, val_rc, val_ra_result;
  
  int slip;
  int exec_lat;                         /* execution latency */
  /* instruction status */
  int dispatched;
  int queued;				/* operands ready and queued */
  int issued;				/* operation is/was executing */
  int completed;			/* operation has completed execution */
  int replayed;				/* operation has been replayed due to load speculation */
  /* output operand dependency list, these lists are used to
     limit the number of associative searches into the ROB when
     instructions complete and need to wake up dependent insts */
  int onames[MAX_ODEPS];		/* output logical names (NA=unused) */

  int context_id;                       /* the id of the context this entry belongs to */
  int iq_entry_num;                     /* the IQ entry number allocated to this entry (or -1 if no entry is allocated) */
  int in_IQ;                            /* flag - is the instruction currently in the IQ? */
  long long disp_cycle;                 /* the cycle this instruction was dispatched */
  long long rename_cycle;               /* the cycle this instruction was renameded */


  int physreg;                          /* the physical register assigned for the instructions destination (-1 if NA) */
  int old_physreg;                      /* the physical register assigned for the instructions destination (-1 if NA) */
  int src_physreg[2];                   /* physical register sources for the inst. (-1 if NA) */

  enum reg_type dest_format;            /* the type of destination register used (none, int, or fp) */

  int archreg;                          /* the architectural register destination (0 if NA) */
  int src_archreg[2];                   /* the architectural source registers (0 if NA) */
};


/*
 * Allocates a physical register to the specified ROB entry
 */
static int alloc_physreg(struct ROB_entry* rob_entry){
  struct reg_set my_regs;
  get_reg_set(&my_regs, rob_entry->op);

  /* store the old destination register mapping */
  rob_entry->old_physreg = contexts[rob_entry->context_id].rename_table[rob_entry->archreg];
  
  /* find a new physreg */
  rob_entry->physreg = find_free_physreg(my_regs.dest);
  assert(rob_entry->physreg >= 0);
 
  /* mark the new physreg as allocated  */
  if(my_regs.dest == REG_INT){
    assert(int_reg_file[rob_entry->physreg].state == REG_FREE);
    int_reg_file[rob_entry->physreg].state = REG_ALLOC;
    int_reg_file[rob_entry->physreg].alloc_cycle = sim_cycle;
    int_reg_file[rob_entry->physreg].spec_ready = INF;
    int_reg_file[rob_entry->physreg].ready = INF;
    assert(rob_entry->physreg >= 0);
  }
  else{
    assert(fp_reg_file[rob_entry->physreg].state == REG_FREE);
    fp_reg_file[rob_entry->physreg].state = REG_ALLOC;
    fp_reg_file[rob_entry->physreg].alloc_cycle = sim_cycle;
    fp_reg_file[rob_entry->physreg].spec_ready = INF;
    fp_reg_file[rob_entry->physreg].ready = INF;
    assert(rob_entry->physreg >= 0);
  }
  
  /* update the rename table */
  contexts[rob_entry->context_id].rename_table[rob_entry->archreg] = rob_entry->physreg;

  assert(rob_entry->physreg >= 0);

  /* return the physical register number */
  return rob_entry->physreg;
}

/******************************************************/

/* non-zero if all register operands are ready */
static int operand_ready(struct ROB_entry *rs, int op_num);
static int operand_spec_ready(struct ROB_entry *rs, int op_num);

static int all_operands_ready(struct ROB_entry *rs);
static int all_operands_spec_ready(struct ROB_entry *rs);

static int one_operand_ready(struct ROB_entry *rs);

/* allocate and initialize ROB */
static void
rob_init(int context_id)
{
  contexts[context_id].ROB = calloc(ROB_size, sizeof(struct ROB_entry));
  if (!contexts[context_id].ROB)
    fatal("out of virtual memory");

  contexts[context_id].ROB_num = 0;
  contexts[context_id].ROB_head = contexts[context_id].ROB_tail = 0;

}

/* dump the contents of the ROB */
static void
rob_dumpent(struct ROB_entry *rs,		/* ptr to ROB entry */
	    int index,				/* entry index */
	    FILE *stream,			/* output stream */
	    int header)				/* print header? */
{
  if (!stream)
    stream = stderr;

  if (header)
    fprintf(stream, "idx: %2d: opcode: %s, inst: `",
	    index, MD_OP_NAME(rs->op));
  else
    fprintf(stream, "       opcode: %s, inst: `",
	    MD_OP_NAME(rs->op));
  md_print_insn(rs->IR, rs->PC, stream);
  fprintf(stream, "'\n");
  myfprintf(stream, "         PC: 0x%08p, NPC: 0x%08p (pred_PC: 0x%08p)\n",
	    rs->PC, rs->next_PC, rs->pred_PC);
  fprintf(stream, "         in_LSQ: %s, ea_comp: %s, recover_inst: %s\n",
	  rs->in_LSQ ? "t" : "f",
	  rs->ea_comp ? "t" : "f",
	  rs->recover_inst ? "t" : "f");
  myfprintf(stream, "         spec_mode: %s, addr: 0x%08p, tag: 0x%08x\n",
	    rs->spec_mode ? "t" : "f", rs->addr, rs->tag);
  fprintf(stream, "         seq: 0x%08x, ptrace_seq: 0x%08x\n",
	  rs->seq, rs->ptrace_seq);
  fprintf(stream, "         queued: %s, issued: %s, completed: %s\n",
	  rs->queued ? "t" : "f",
	  rs->issued ? "t" : "f",
	  rs->completed ? "t" : "f");
  fprintf(stream, "         operands ready: %s\n",
	  all_operands_ready(rs) ? "t" : "f");
}

/* dump the contents of the ROB */
static void
rob_dump(FILE *stream, int context_id)				/* output stream */
{
  int num, head;
  struct ROB_entry *rs;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** ROB state **\n");
  fprintf(stream, "ROB_head: %d, ROB_tail: %d\n", contexts[context_id].ROB_head, contexts[context_id].ROB_tail);
  fprintf(stream, "ROB_num: %d\n", contexts[context_id].ROB_num);

  num = contexts[context_id].ROB_num;
  head = contexts[context_id].ROB_head;
  while (num)
    {
      rs = &contexts[context_id].ROB[head];
      rob_dumpent(rs, rs - contexts[context_id].ROB, stream, /* header */TRUE);
      head = (head + 1) % ROB_size;
      num--;
    }
}


/*
 * input dependencies for stores in the LSQ:
 *   idep #0 - operand input (value that is store'd)
 *   idep #1 - effective address input (address of store operation)
 */
#define STORE_OP_INDEX                  0
#define STORE_ADDR_INDEX                1

#define STORE_OP_READY(RS)              (operand_spec_ready(RS, STORE_OP_INDEX))
#define STORE_ADDR_READY(RS)            (operand_spec_ready(RS, STORE_ADDR_INDEX))




/* allocate and initialize the load/store queue (LSQ) */
static void
lsq_init(int context_id)
{
  contexts[context_id].LSQ = calloc(LSQ_size, sizeof(struct ROB_entry));
  if (!contexts[context_id].LSQ)
    fatal("out of virtual memory");

  contexts[context_id].LSQ_num = 0;
  contexts[context_id].LSQ_head = contexts[context_id].LSQ_tail = 0;
}

/* dump the contents of the ROB */
static void
lsq_dump(FILE *stream, int context_id)				/* output stream */
{
  int num, head;
  struct ROB_entry *rs;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** LSQ state **\n");
  fprintf(stream, "LSQ_head: %d, LSQ_tail: %d\n", contexts[context_id].LSQ_head, contexts[context_id].LSQ_tail);
  fprintf(stream, "LSQ_num: %d\n", contexts[context_id].LSQ_num);

  num = contexts[context_id].LSQ_num;
  head = contexts[context_id].LSQ_head;
  while (num)
    {
      rs = &contexts[context_id].LSQ[head];
      rob_dumpent(rs, rs - contexts[context_id].LSQ, stream, /* header */TRUE);
      head = (head + 1) % LSQ_size;
      num--;
    }
}


/*
 * RS_LINK defs and decls
 */

/* an ROB Link: this structure links elements of an ROB entry;
   used for ready instruction queue, event queue, and
   output dependency lists; each RS_LINK node contains a pointer to the ROB
   entry it references along with an instance tag, the RS_LINK is only valid if
   the instruction instance tag matches the instruction ROB entry instance tag;
   this strategy allows entries in the ROB can be squashed and reused without
   updating the lists that point to it, which significantly improves the
   performance of (all to frequent) squash events */
struct RS_link {
  struct RS_link *next;			/* next entry in list */
  struct ROB_entry *rs;		        /* referenced ROB entry */
  INST_TAG_TYPE tag;			/* inst instance sequence number */
  union {
    tick_t when;			/* time stamp of entry (for eventq) */
    INST_SEQ_TYPE seq;			/* inst sequence */
    int opnum;				/* input/output operand number */
  } x;
};

/* RS link free list, grab RS_LINKs from here, when needed */
static struct RS_link *rslink_free_list;

/* NULL value for an RS link */
#define RSLINK_NULL_DATA		{ NULL, NULL, 0 }

/* create and initialize an RS link */
#define RSLINK_INIT(RSL, RS)						\
  ((RSL).next = NULL, (RSL).rs = (RS), (RSL).tag = (RS)->tag)

/* non-zero if RS link is NULL */
#define RSLINK_IS_NULL(LINK)            ((LINK)->rs == NULL)

/* non-zero if RS link is to a valid (non-squashed) entry */
#define RSLINK_VALID(LINK)              ((LINK)->tag == (LINK)->rs->tag)

/* extra ROB entry pointer */
#define RSLINK_RS(LINK)                 ((LINK)->rs)

/* get a new RS link record */
#define RSLINK_NEW(DST, RS)						\
  { struct RS_link *n_link;						\
    if (!rslink_free_list)						\
      panic("out of rs links");						\
    n_link = rslink_free_list;						\
    rslink_free_list = rslink_free_list->next;				\
    n_link->next = NULL;						\
    n_link->rs = (RS); n_link->tag = n_link->rs->tag;			\
    (DST) = n_link;							\
  }

/* free an RS link record */
#define RSLINK_FREE(LINK)						\
  {  struct RS_link *f_link = (LINK);					\
     f_link->rs = NULL; f_link->tag = 0;				\
     f_link->next = rslink_free_list;					\
     rslink_free_list = f_link;						\
  }

/* FIXME: could this be faster!!! */
/* free an RS link list */
#define RSLINK_FREE_LIST(LINK)						\
  {  struct RS_link *fl_link, *fl_link_next;				\
     for (fl_link=(LINK); fl_link; fl_link=fl_link_next)		\
       {								\
	 fl_link_next = fl_link->next;					\
	 RSLINK_FREE(fl_link);						\
       }								\
  }

/* initialize the free RS_LINK pool */
static void
rslink_init(int nlinks)			/* total number of RS_LINK available */
{
  int i;
  struct RS_link *link;

  rslink_free_list = NULL;
  for (i=0; i<nlinks; i++)
    {
      link = calloc(1, sizeof(struct RS_link));
      if (!link)
	fatal("out of virtual memory");
      link->next = rslink_free_list;
      rslink_free_list = link;
    }
}

/* service all functional unit release events, this function is called
   once per cycle, and it used to step the BUSY timers attached to each
   functional unit in the function unit resource pool, as long as a functional
   unit's BUSY count is > 0, it cannot be issued an operation */
static void
rob_release_fu(void)
{
  int i;

  /* walk all resource units, decrement busy counts by one */
  for (i=0; i<fu_pool->num_resources; i++)
    {
      /* resource is released when BUSY hits zero */
      if (fu_pool->resources[i].busy > 0)
	fu_pool->resources[i].busy--;
    }
}


/*
 * the execution unit event queue implementation follows, the event queue
 * indicates which instruction will complete next, the writeback handler
 * drains this queue
 */

/* pending event queue, sorted from soonest to latest event (in time), NOTE:
   RS_LINK nodes are used for the event queue list so that it need not be
   updated during squash events */
static struct RS_link *event_queue;

/* initialize the event queue structures */
static void
eventq_init(void)
{
  event_queue = NULL;
}

/* dump the contents of the event queue */
static void
eventq_dump(FILE *stream, int context_id)			/* output stream */
{
  struct RS_link *ev;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** event queue state **\n");

  for (ev = event_queue; ev != NULL; ev = ev->next)
    {
      /* is event still valid? */
      if (RSLINK_VALID(ev))
	{
	  struct ROB_entry *rs = RSLINK_RS(ev);

	  fprintf(stream, "idx: %2d: @ %.0f\n",
		  (int)(rs - (rs->in_LSQ ? contexts[context_id].LSQ : contexts[context_id].ROB)), (double)ev->x.when);
	  rob_dumpent(rs, rs - (rs->in_LSQ ? contexts[context_id].LSQ : contexts[context_id].ROB),
		      stream, /* !header */FALSE);
	}
    }
}

/* insert an event for RS into the event queue, event queue is sorted from
   earliest to latest event, event and associated side-effects will be
   apparent at the start of cycle WHEN */
static void
eventq_queue_event(struct ROB_entry *rs, tick_t when)
{
  struct RS_link *prev, *ev, *new_ev;

  if (rs->completed)
    panic("event completed");

  if (when <= sim_cycle)
    panic("event occurred in the past");

  /* get a free event record */
  RSLINK_NEW(new_ev, rs);
  new_ev->x.when = when;

  /* locate insertion point */
  for (prev=NULL, ev=event_queue;
       ev && ev->x.when < when;
       prev=ev, ev=ev->next);

  if (prev)
    {
      /* insert middle or end */
      new_ev->next = prev->next;
      prev->next = new_ev;
    }
  else
    {
      /* insert at beginning */
      new_ev->next = event_queue;
      event_queue = new_ev;
    }
}

/* return the next event that has already occurred, returns NULL when no
   remaining events or all remaining events are in the future */
static struct ROB_entry *
eventq_next_event(void)
{
  struct RS_link *ev;

  if (event_queue && event_queue->x.when <= sim_cycle)
    {
      /* unlink and return first event on priority list */
      ev = event_queue;
      event_queue = event_queue->next;

      /* event still valid? */
      if (RSLINK_VALID(ev))
	{
	  struct ROB_entry *rs = RSLINK_RS(ev);

	  /* reclaim event record */
	  RSLINK_FREE(ev);

	  /* event is valid, return ROB entry */
	  return rs;
	}
      else
	{
	  /* reclaim event record */
	  RSLINK_FREE(ev);

	  /* receiving inst was squashed, return next event */
	  return eventq_next_event();
	}
    }
  else
    {
      /* no event or no event is ready */
      return NULL;
    }
}





/* Holds instructions between issue and execution */
static struct RS_link * issue_exec_queue;

/* initialize the queue structures */
static void
issue_exec_q_init(void)
{
  issue_exec_queue = NULL;
}

static void
issue_exec_q_queue_event(struct ROB_entry *rs, tick_t when)
{
  struct RS_link *prev, *ev, *new_ev;

  if (rs->completed)
    panic("instruction completed");

  assert(all_operands_spec_ready(rs));

  if (when < sim_cycle)
    panic("event occurred in the past");

  /* get a free event record */
  RSLINK_NEW(new_ev, rs);
  new_ev->x.when = when;

  /* locate insertion point */
  for (prev=NULL, ev=issue_exec_queue;
       ev && ev->x.when < when;
       prev=ev, ev=ev->next);

  if (prev)
    {
      /* insert middle or end */
      new_ev->next = prev->next;
      prev->next = new_ev;
    }
  else
    {
      /* insert at beginning */
      new_ev->next = issue_exec_queue;
      issue_exec_queue = new_ev;
    }
}

static struct ROB_entry *
issue_exec_q_next_event(void)
{
  struct RS_link *ev;

  if (issue_exec_queue && issue_exec_queue->x.when <= sim_cycle)
    {
      /* unlink and return first event on priority list */
      ev = issue_exec_queue;
      issue_exec_queue = issue_exec_queue->next;

      /* event still valid? */
      if (RSLINK_VALID(ev))
	{
	  struct ROB_entry *rs = RSLINK_RS(ev);

	  /* reclaim event record */
	  RSLINK_FREE(ev);

	  /* event is valid, return resv station */
	  return rs;
	}
      else
	{
	  /* reclaim event record */
	  RSLINK_FREE(ev);

	  /* receiving inst was squashed, return next event */
	  return issue_exec_q_next_event();
	}
    }
  else
    {
      /* no event or no event is ready */
      return NULL;
    }
}


/*
 * waiting instruction queue - instructions wait here until all their source
 * operands are marked as ready
 */

/* the ready instruction queue */
static struct RS_link *waiting_queue;

/* initialize the event queue structures */
static void
waitq_init(void)
{
  waiting_queue = NULL;
}

static void
wait_q_enqueue(struct ROB_entry *rs, tick_t when)
{
  struct RS_link *prev, *ev, *new_ev;

  if (rs->completed)
    panic("instruction completed");

  assert(!rs->queued);

  if (when < sim_cycle)
    panic("event occurred in the past");

  /* get a free event record */
  RSLINK_NEW(new_ev, rs);
  new_ev->x.when = when;

  /* locate insertion point */
  for (prev=NULL, ev=waiting_queue;
       ev && ev->x.when < when;
       prev=ev, ev=ev->next);

  if (prev)
    {
      /* insert middle or end */
      new_ev->next = prev->next;
      prev->next = new_ev;
    }
  else
    {
      /* insert at beginning */
      new_ev->next = waiting_queue;
      waiting_queue = new_ev;
    }
}

/*
 * the ready instruction queue implementation follows, the ready instruction
 * queue indicates which instruction have all of there *register* dependencies
 * satisfied, instruction will issue when 1) all memory dependencies for
 * the instruction have been satisfied (see lsq_refresh() for details on how
 * this is accomplished) and 2) resources are available; ready queue is fully
 * constructed each cycle before any operation is issued from it -- this
 * ensures that instruction issue priorities are properly observed; NOTE:
 * RS_LINK nodes are used for the event queue list so that it need not be
 * updated during squash events
 */

/* the ready instruction queue */
static struct RS_link *ready_queue;

/* initialize the event queue structures */
static void
readyq_init(void)
{
  ready_queue = NULL;
}

/* dump the contents of the ready queue */
static void
readyq_dump(FILE *stream, int context_id)			/* output stream */
{
  struct RS_link *link;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** ready queue state **\n");

  for (link = ready_queue; link != NULL; link = link->next)
    {
      /* is entry still valid? */
      if (RSLINK_VALID(link))
	{
	  struct ROB_entry *rs = RSLINK_RS(link);

	  rob_dumpent(rs, rs - (rs->in_LSQ ? contexts[context_id].LSQ : contexts[context_id].ROB),
		      stream, /* header */TRUE);
	}
    }
}

/* insert ready node into the ready list using ready instruction scheduling
   policy; currently the following scheduling policy is enforced:

     memory and long latency operands, and branch instructions first

   then

     all other instructions, oldest instructions first

  this policy works well because branches pass through the machine quicker
  which works to reduce branch misprediction latencies, and very long latency
  instructions (such loads and multiplies) get priority since they are very
  likely on the program's critical path */
static void
readyq_enqueue(struct ROB_entry *rs)		/* RS to enqueue */
{
  struct RS_link *prev, *node, *new_node;

  /* node is now queued */
  if (rs->queued)
    panic("node is already queued");
  rs->queued = TRUE;


  assert(all_operands_spec_ready(rs));


  /* get a free ready list node */
  RSLINK_NEW(new_node, rs);
  new_node->x.seq = rs->seq;

  /* otherwise insert in program order (earliest seq first) */
  /* OLDEST FIRST ISSUE */
  for (prev=NULL, node=ready_queue;
     node && node->x.seq < rs->seq;
     prev=node, node=node->next);

  if (prev)
    {
      /* insert middle or end */
      new_node->next = prev->next;
      prev->next = new_node;
    }
  else
    {
      /* insert at beginning */
      new_node->next = ready_queue;
      ready_queue = new_node;
    }
}


/*
 *  COMMIT() - instruction retirement pipeline stage
 */

/* this function commits the results of the oldest completed entries from the
   ROB and LSQ to the architected reg file, stores in the LSQ will commit
   their store data to the data cache at this point as well */
static void
commit(int context_id)
{
  int lat, events, committed = 0;
  static counter_t sim_ret_insn = 0;
  
  if((last_commit_cycle + COMMIT_TIMEOUT + 1) <= sim_cycle){
    printf("NO INSTRUCTIONS HAVE COMMITED IN A LONG TIME %lld %lld\n", last_commit_cycle, sim_cycle);
    assert(FALSE);
  }

  if((last_commit_cycle + COMMIT_TIMEOUT) <= sim_cycle){
    printf("%d\n", contexts[context_id].ROB_num);
  }  


  /* all values must be retired to the architected reg file in program order */
  while (contexts[context_id].ROB_num > 0 && committed < commit_width)
    {
      struct ROB_entry *rs = &(contexts[context_id].ROB[contexts[context_id].ROB_head]);

      if (!rs->completed)
	{
	  if((last_commit_cycle + COMMIT_TIMEOUT) <= sim_cycle){
	    fprintf(stderr, "NOT COMLPETED: \n");
	    md_print_insn(rs->IR, rs->PC, stderr);
	    fprintf(stderr, "\n %d %d\n", rs->dispatched, find_iq_entry(2));
	  }
	  break;
	}

      /* default commit events */
      events = 0;

      /* load/stores must retire load/store queue entry as well */
      if (contexts[context_id].ROB[contexts[context_id].ROB_head].ea_comp)
	{
	  /* load/store, retire head of LSQ as well */
	  if (contexts[context_id].LSQ_num <= 0 || !contexts[context_id].LSQ[contexts[context_id].LSQ_head].in_LSQ)
	    panic("ROB out of sync with LSQ");

	  /* load/store operation must be complete */
	  if (!contexts[context_id].LSQ[contexts[context_id].LSQ_head].completed)
	    {
	      /* load/store operation is not yet complete */
	      if((last_commit_cycle + COMMIT_TIMEOUT) <= sim_cycle){
		fprintf(stderr, "LSQ NOT COMLPETED: \n");
		md_print_insn(contexts[context_id].LSQ[contexts[context_id].LSQ_head].IR, contexts[context_id].LSQ[contexts[context_id].LSQ_head].PC, stderr);
		fprintf(stderr, "\n");
		fprintf(stderr, "%d \n", contexts[context_id].LSQ[contexts[context_id].LSQ_head].issued);
		fprintf(stderr, "%d \n", rs->completed);
	      }
	      break;
	    }

	  assert(contexts[context_id].ROB[contexts[context_id].ROB_head].context_id == contexts[context_id].LSQ[contexts[context_id].LSQ_head].context_id);

	  if ((MD_OP_FLAGS(contexts[context_id].LSQ[contexts[context_id].LSQ_head].op) & (F_MEM|F_STORE))
	      == (F_MEM|F_STORE))
	    {
	      struct res_template *fu;

	      /* stores must retire their store value to the cache at commit,
		 try to get a store port (functional unit allocation) */
	      fu = res_get(fu_pool, MD_OP_FUCLASS(contexts[context_id].LSQ[contexts[context_id].LSQ_head].op));
	      if (fu)
		{
		  /* reserve the functional unit */
		  if (fu->master->busy)
		    panic("functional unit already in use");

		  /* schedule functional unit release event */
		  fu->master->busy = fu->issuelat;

		  /* go to the data cache */
		  if (cache_dl1)
		    {
		      /* Wattch -- D-cache access */
		      dcache_access++;

		      /* commit store value to D-cache */
		      lat =
			cache_access(cache_dl1, Write, (contexts[context_id].LSQ[contexts[context_id].LSQ_head].addr&~3),
				     context_id, NULL, 4, sim_cycle, NULL, NULL);
		      if (lat > cache_dl1_lat)
			events |= PEV_CACHEMISS;
		    }

		  /* all loads and stores must to access D-TLB */
		  if (dtlb)
		    {
		      /* access the D-TLB */
		      lat =
			cache_access(dtlb, Read, (contexts[context_id].LSQ[contexts[context_id].LSQ_head].addr & ~3),
				     context_id, NULL, 4, sim_cycle, NULL, NULL);
		      if (lat > 1)
			events |= PEV_TLBMISS;
		    }
		}
	      else
		{
		  /* no store ports left, cannot continue to commit insts */
		  break;
		}
	    }


	  /* invalidate load/store operation instance */
	  contexts[context_id].LSQ[contexts[context_id].LSQ_head].tag++;
          sim_slip += (sim_cycle - contexts[context_id].LSQ[contexts[context_id].LSQ_head].slip);
   
	  /* indicate to pipeline trace that this instruction retired */
	  ptrace_newstage(contexts[context_id].LSQ[contexts[context_id].LSQ_head].ptrace_seq, PST_COMMIT, events);
	  ptrace_endinst(contexts[context_id].LSQ[contexts[context_id].LSQ_head].ptrace_seq);

	  /* commit head of LSQ as well */
	  contexts[context_id].LSQ_head = (contexts[context_id].LSQ_head + 1) % LSQ_size;
	  contexts[context_id].LSQ_num--;
	}

      /* Wattch -- committed instruction to arch reg file */
      if ((MD_OP_FLAGS(rs->op) & (F_ICOMP|F_FCOMP)) || ((MD_OP_FLAGS(rs->op) & (F_MEM|F_LOAD)) == (F_MEM|F_LOAD))) {
	regfile_access++;
#ifdef DYNAMIC_AF	
	regfile_total_pop_count_cycle += pop_count(rs->val_rc);
	regfile_num_pop_count_cycle++;
#endif
      }

      if (contexts[context_id].pred
	  && bpred_spec_update == spec_CT
	  && (MD_OP_FLAGS(rs->op) & F_CTRL))
	{
	  /* Wattch -- bpred access */
	  bpred_access++;

	  bpred_update(contexts[context_id].pred,
		       /* branch address */rs->PC,
		       /* actual target address */rs->next_PC,
                       /* taken? */rs->next_PC != (rs->PC +
                                                   sizeof(md_inst_t)),
                       /* pred taken? */rs->pred_PC != (rs->PC +
                                                        sizeof(md_inst_t)),
                       /* correct pred? */rs->pred_PC == rs->next_PC,
                       /* opcode */rs->op,
                       /* dir predictor update pointer */&rs->dir_update);
	}

      /* invalidate ROB operation instance */
      contexts[context_id].ROB[contexts[context_id].ROB_head].tag++;
      sim_slip += (sim_cycle - contexts[context_id].ROB[contexts[context_id].ROB_head].slip);
      /* print retirement trace if in verbose mode */
      if (verbose)
	{
	  sim_ret_insn++;
	  myfprintf(stderr, "%10n @ 0x%08p: ", sim_ret_insn, contexts[context_id].ROB[contexts[context_id].ROB_head].PC);
 	  md_print_insn(contexts[context_id].ROB[contexts[context_id].ROB_head].IR, contexts[context_id].ROB[contexts[context_id].ROB_head].PC, stderr);
	  if (MD_OP_FLAGS(contexts[context_id].ROB[contexts[context_id].ROB_head].op) & F_MEM)
	    myfprintf(stderr, "  mem: 0x%08p", contexts[context_id].ROB[contexts[context_id].ROB_head].addr);
	  fprintf(stderr, "\n");
	  /* fflush(stderr); */
	}

      /* indicate to pipeline trace that this instruction retired */
      ptrace_newstage(contexts[context_id].ROB[contexts[context_id].ROB_head].ptrace_seq, PST_COMMIT, events);
      ptrace_endinst(contexts[context_id].ROB[contexts[context_id].ROB_head].ptrace_seq);

      /* update # instructions committed for this thread */
      contexts[context_id].sim_num_insn++;

      /* hangle register deallocations */
      if(contexts[context_id].ROB[contexts[context_id].ROB_head].physreg >= 0){
	/* free the old physreg mapping */
	if(contexts[context_id].ROB[contexts[context_id].ROB_head].dest_format == REG_INT){
	  assert(int_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].old_physreg].state == REG_ARCH);
	  int_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].old_physreg].state = REG_FREE;
	}else{
	  assert(contexts[context_id].ROB[contexts[context_id].ROB_head].dest_format == REG_FP);
	  assert(fp_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].old_physreg].state == REG_ARCH);
	  fp_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].old_physreg].state = REG_FREE;
	}
      }
      
      if(contexts[context_id].ROB[contexts[context_id].ROB_head].physreg >= 0){
	/* commit the physreg mapping to arch state */
	if(contexts[context_id].ROB[contexts[context_id].ROB_head].dest_format == REG_INT){
	  assert(int_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].physreg].state == REG_WB);
	  int_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].physreg].state = REG_ARCH;
	}else{
	  assert(contexts[context_id].ROB[contexts[context_id].ROB_head].dest_format == REG_FP);
	  assert(fp_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].physreg].state == REG_WB);
	  fp_reg_file[contexts[context_id].ROB[contexts[context_id].ROB_head].physreg].state = REG_ARCH;
	}
      }
    

      /* commit head entry of ROB */
      contexts[context_id].ROB_head = (contexts[context_id].ROB_head + 1) % ROB_size;
      contexts[context_id].ROB_num--;

      /* one more instruction committed to architected state */
      committed++;
      last_commit_cycle = sim_cycle;

    }
}


/*
 *  ROB_RECOVER() - squash mispredicted microarchitecture state
 */

/* recover processor microarchitecture state back to point of the
   mis-predicted branch at ROB[BRANCH_INDEX] */
static void
rob_recover(int branch_index, int context_id)			/* index of mis-pred branch */
{
  int ROB_index = contexts[context_id].ROB_tail, LSQ_index = contexts[context_id].LSQ_tail;
  int ROB_prev_tail = contexts[context_id].ROB_tail, LSQ_prev_tail = contexts[context_id].LSQ_tail;

  /* recover from the tail of the ROB towards the head until the branch index
     is reached, this direction ensures that the LSQ can be synchronized with
     the ROB */

  /* go to first element to squash */
  ROB_index = (ROB_index + (ROB_size-1)) % ROB_size;
  LSQ_index = (LSQ_index + (LSQ_size-1)) % LSQ_size;
  /* traverse to older insts until the mispredicted branch is encountered */
  while (ROB_index != branch_index)
    {
      /* the ROB should not drain since the mispredicted branch will remain */
      if (!contexts[context_id].ROB_num)
	panic("empty ROB");

      /* should meet up with the tail first */
      if (ROB_index == contexts[context_id].ROB_head)
	panic("ROB head and tail broken");

      /* is this operation an effective addr calc for a load or store? */
      if (contexts[context_id].ROB[ROB_index].ea_comp)
	{
	  /* should be at least one load or store in the LSQ */
	  if (!contexts[context_id].LSQ_num)
	    panic("ROB and LSQ out of sync");

	  /* squash this LSQ entry */
	  contexts[context_id].LSQ[LSQ_index].tag++;

	  /* indicate in pipetrace that this instruction was squashed */
	  ptrace_endinst(contexts[context_id].LSQ[LSQ_index].ptrace_seq);

	  assert(contexts[context_id].LSQ[LSQ_index].physreg == contexts[context_id].ROB[ROB_index].physreg);


	  /* go to next earlier LSQ slot */
	  LSQ_prev_tail = LSQ_index;
	  LSQ_index = (LSQ_index + (LSQ_size-1)) % LSQ_size;
	  contexts[context_id].LSQ_num--;
	}

      /* squash this ROB entry */
      contexts[context_id].ROB[ROB_index].tag++;

      /* indicate in pipetrace that this instruction was squashed */
      ptrace_endinst(contexts[context_id].ROB[ROB_index].ptrace_seq);

      /* if the instruction is in the IQ, then free the IQ entry */
      if(contexts[context_id].ROB[ROB_index].in_IQ >= 1){
	free_iq_entry(contexts[context_id].ROB[ROB_index].iq_entry_num);
	contexts[context_id].ROB[ROB_index].in_IQ = FALSE;
	contexts[context_id].icount--;
	assert(contexts[context_id].icount >= 0);
      }

      if(!contexts[context_id].ROB[ROB_index].dispatched)
	contexts[context_id].icount--;

      /* release physical registers */
      if(contexts[context_id].ROB[ROB_index].physreg >= 0){
	if(contexts[context_id].ROB[ROB_index].dest_format == REG_INT){
	  assert(int_reg_file[contexts[context_id].ROB[ROB_index].physreg].state != REG_FREE);
	  assert(int_reg_file[contexts[context_id].ROB[ROB_index].physreg].state != REG_ARCH);
	  int_reg_file[contexts[context_id].ROB[ROB_index].physreg].state = REG_FREE;
	}else{
	  assert(contexts[context_id].ROB[ROB_index].dest_format == REG_FP);
	  assert(fp_reg_file[contexts[context_id].ROB[ROB_index].physreg].state != REG_FREE);
	  assert(fp_reg_file[contexts[context_id].ROB[ROB_index].physreg].state != REG_ARCH);
	  fp_reg_file[contexts[context_id].ROB[ROB_index].physreg].state = REG_FREE;
	}
	/* rollback the rename table */
	contexts[context_id].rename_table[contexts[context_id].ROB[ROB_index].archreg] = contexts[context_id].ROB[ROB_index].old_physreg;
	contexts[context_id].ROB[ROB_index].physreg = -1;
      }
     
      /* go to next earlier slot in the ROB */
      ROB_prev_tail = ROB_index;
      ROB_index = (ROB_index + (ROB_size-1)) % ROB_size;
      contexts[context_id].ROB_num--;
    }
  /* reset head/tail pointers to point to the mis-predicted branch */
  contexts[context_id].ROB_tail = ROB_prev_tail;
  contexts[context_id].LSQ_tail = LSQ_prev_tail;
  
  /* FIXME: could reset functional units at squash time */
}


/*
 *  WRITEBACK() - instruction result writeback pipeline stage
 */

/* forward declarations */
static void tracer_recover(int context_id);


/* writeback completed operation results from the functional units to ROB,
   at this point, the output dependency chains of completing instructions
   are also walked to determine if any dependent instruction now has all
   of its register operands, if so the (nearly) ready instruction is inserted
   into the ready instruction queue */
static void
writeback(void)
{
  struct ROB_entry *rs;

  /* service all completed events */
  while ((rs = eventq_next_event()))
    {
      /* RS has completed execution and (possibly) produced a result */
      if (!all_operands_ready(rs) || rs->queued || !rs->issued || rs->completed)
	panic("inst completed and !ready, !issued, or completed");
      
      /* operation has completed */
      rs->completed = TRUE;


      /* Wattch -- 1) Writeback result to resultbus 
                   2) Write result to phys. regs (RUU)
		   3) Access wakeup logic
       */
      if(!(MD_OP_FLAGS(rs->op) & F_CTRL)) {
	window_access++;
	window_preg_access++;
	window_wakeup_access++;
	resultbus_access++;
#ifdef DYNAMIC_AF	
	window_total_pop_count_cycle += pop_count(rs->val_rc);
	window_num_pop_count_cycle++;
	resultbus_total_pop_count_cycle += pop_count(rs->val_rc);
	resultbus_num_pop_count_cycle++;
#endif
      }

      /* mark the destination physreg as written back */
      if((rs->physreg >= 0) && (!rs->ea_comp)){
	if(rs->dest_format == REG_INT){

	  // see if its a load that mispredicted
	  // NOT A GREAT FIX.......
	  if(int_reg_file[rs->physreg].ready != sim_cycle){
	    md_print_insn(rs->IR, rs->PC, stderr);
	    fprintf(stderr, "\n %lld %d %d %d\n",int_reg_file[rs->physreg].ready, rs->in_LSQ, rs->replayed, rs->ptrace_seq);
	    int_reg_file[rs->physreg].ready = sim_cycle;
	    assert(FALSE);
	  }

	  assert(int_reg_file[rs->physreg].ready == sim_cycle);
	  assert(int_reg_file[rs->physreg].state == REG_ALLOC);
	  int_reg_file[rs->physreg].state = REG_WB;
	}else{
	  if(fp_reg_file[rs->physreg].ready != sim_cycle){
	    md_print_insn(rs->IR, rs->PC, stderr);
	    fprintf(stderr, "\n %lld %d %d %d %lld\n",fp_reg_file[rs->physreg].ready, rs->in_LSQ, rs->replayed, rs->ptrace_seq, sim_cycle);
	    fp_reg_file[rs->physreg].ready = sim_cycle;
	  }
	  assert(fp_reg_file[rs->physreg].ready == sim_cycle);
	  if(rs->dest_format != REG_FP){
	    md_print_insn(rs->IR, rs->PC, stderr);
	    printf("\n");
	  }
	  assert(rs->dest_format == REG_FP);
	  assert(fp_reg_file[rs->physreg].state == REG_ALLOC);
	  fp_reg_file[rs->physreg].state = REG_WB;
	}
      }

      /* does this operation reveal a mis-predicted branch? */
      if (rs->recover_inst)
	{
	  if (rs->in_LSQ)
	    panic("mis-predicted load or store?!?!?");
	  /* recover processor state and reinit fetch to correct path */
	  rob_recover(rs - contexts[rs->context_id].ROB, rs->context_id);
	  tracer_recover(rs->context_id);
	  bpred_recover(contexts[rs->context_id].pred, rs->PC, rs->stack_recover_idx);

	  /* continue writeback of the branch/control instruction */
	}

      /* if we speculatively update branch-predictor, do it here */
      if (contexts[rs->context_id].pred
	  && bpred_spec_update == spec_WB
	  && !rs->in_LSQ
	  && (MD_OP_FLAGS(rs->op) & F_CTRL))
	{
	  /* Wattch -- bpred access */
	  bpred_access++;

	  bpred_update(contexts[rs->context_id].pred,
		       /* branch address */rs->PC,
		       /* actual target address */rs->next_PC,
		       /* taken? */rs->next_PC != (rs->PC +
						   sizeof(md_inst_t)),
		       /* pred taken? */rs->pred_PC != (rs->PC +
							sizeof(md_inst_t)),
		       /* correct pred? */rs->pred_PC == rs->next_PC,
		       /* opcode */rs->op,
		       /* dir predictor update pointer */&rs->dir_update);
	}

      /* entered writeback stage, indicate in pipe trace */
      ptrace_newstage(rs->ptrace_seq, PST_WRITEBACK,
		      rs->recover_inst ? PEV_MPDETECT : 0);


   } /* for all writeback events */

}


/*
 *  LSQ_REFRESH() - memory access dependence checker/scheduler
 */

/* this function locates ready instructions whose memory dependencies have
   been satisfied, this is accomplished by walking the LSQ for loads, looking
   for blocking memory dependency condition (e.g., earlier store with an
   unknown address) */
#define MAX_STD_UNKNOWNS		256
static void
lsq_refresh(int context_id)
{
  int i, j, index, n_std_unknowns;
  md_addr_t std_unknowns[MAX_STD_UNKNOWNS];

  /* scan entire queue for ready loads: scan from oldest instruction
     (head) until we reach the tail or an unresolved store, after which no
     other instruction will become ready */
  for (i=0, index=contexts[context_id].LSQ_head, n_std_unknowns=0;
       i < contexts[context_id].LSQ_num;
       i++, index=(index + 1) % LSQ_size)
    {
      /* terminate search for ready loads after first unresolved store,
	 as no later load could be resolved in its presence */
      if (/* store? */
	  (MD_OP_FLAGS(contexts[context_id].LSQ[index].op) & (F_MEM|F_STORE)) == (F_MEM|F_STORE))
	{
	  if (!STORE_ADDR_READY(&contexts[context_id].LSQ[index]))
	    {
	      /* FIXME: a later STD + STD known could hide the STA unknown */
	      /* sta unknown, blocks all later loads, stop search */
	      break;
	    }
	  else if (!all_operands_spec_ready(&contexts[context_id].LSQ[index]))
	    {
	      /* sta known, but std unknown, may block a later store, record
		 this address for later referral, we use an array here because
		 for most simulations the number of entries to search will be
		 very small */
	      if (n_std_unknowns == MAX_STD_UNKNOWNS)
		fatal("STD unknown array overflow, increase MAX_STD_UNKNOWNS");
	      std_unknowns[n_std_unknowns++] = contexts[context_id].LSQ[index].addr;
	    }
	  else /* STORE_ADDR_READY() && OPERANDS_READY() */
	    {
	      /* a later STD known hides an earlier STD unknown */
	      for (j=0; j<n_std_unknowns; j++)
		{
		  if (std_unknowns[j] == /* STA/STD known */contexts[context_id].LSQ[index].addr)
		    std_unknowns[j] = /* bogus addr */0;
		}
	    }
	}

      if (/* load? */
	  ((MD_OP_FLAGS(contexts[context_id].LSQ[index].op) & (F_MEM|F_LOAD)) == (F_MEM|F_LOAD))
	  && contexts[context_id].LSQ[index].dispatched
	  && /* queued? */!contexts[context_id].LSQ[index].queued
	  && /* waiting? */!contexts[context_id].LSQ[index].issued
	  && /* completed? */!contexts[context_id].LSQ[index].completed
	  && /* regs ready? */all_operands_spec_ready(&contexts[context_id].LSQ[index]))
	{
	  
	  /* no STA unknown conflict (because we got to this check), check for
	     a STD unknown conflict */
	  for (j=0; j<n_std_unknowns; j++)
	    {
	      /* found a relevant STD unknown? */
	      if (std_unknowns[j] == contexts[context_id].LSQ[index].addr)
		break;
	    }
	  if (j == n_std_unknowns)
	    {
	      /* no STA or STD unknown conflicts, put load on ready queue */
	      readyq_enqueue(&contexts[context_id].LSQ[index]);
	    }
	}

    }
}




#define PRED_BIMOD

/* takes instructions out of the issue_exec_q and begins their execution
 * schedules a writeback event
*/
static void
execute(void)
{
  struct ROB_entry *rs;
  /* service all completed events */
  while ((rs = issue_exec_q_next_event()))
    {
      if(!all_operands_ready(rs)){
	/* handle memory misprediciton! */
	if(!mystricmp(recovery_model, "squash")){
	  struct RS_link *node, *next_node;
	  node = issue_exec_queue;
	  issue_exec_queue = NULL;
	  
	  //the first one is the direct dependent on the load, so fixt its source...
	  {
	    struct reg_set my_regs;
	    get_reg_set(&my_regs, rs->op);
	    
	    if(rs->src_physreg[0] > 0){
	      switch(my_regs.src1){
	      case REG_INT:
		int_reg_file[rs->src_physreg[0]].spec_ready = int_reg_file[rs->src_physreg[0]].ready - ISSUE_EXEC_DELAY;
		break;
	      case REG_FP:
		fp_reg_file[rs->src_physreg[0]].spec_ready = fp_reg_file[rs->src_physreg[0]].ready - ISSUE_EXEC_DELAY;
		break;
	      case REG_NONE:
		break;
	      }
	    }
	    if(rs->src_physreg[1] > 0){
	      switch(my_regs.src2){
	      case REG_INT:
		int_reg_file[rs->src_physreg[1]].spec_ready = int_reg_file[rs->src_physreg[1]].ready - ISSUE_EXEC_DELAY;
		break;
	      case REG_FP:
		fp_reg_file[rs->src_physreg[1]].spec_ready = fp_reg_file[rs->src_physreg[1]].ready - ISSUE_EXEC_DELAY;
		break;
	      case REG_NONE:
		break;
	      }
	    }
	  }
	  
	  
	  while(rs){
	    if((rs->physreg >= 0) && (!rs->ea_comp)){
	      //restet REG counters....
	      switch(rs->dest_format){
	      case REG_INT:
		/* earliest cycle dependents should wakeup */
		int_reg_file[rs->physreg].spec_ready = INF;
		/* data will be on the bypass network when this instruction completes execution */
		int_reg_file[rs->physreg].ready = INF;
		break;
		
	      case REG_FP:
		/* earliest cycle dependents should wakeup */
		fp_reg_file[rs->physreg].spec_ready = INF;
		/* data will be on the bypass network when this instruction completes execution */
		fp_reg_file[rs->physreg].ready = INF;
		break;
		
	      case REG_NONE:
		break;
	      };
	    }
	    /* mark it as not issued */
	    rs->issued = FALSE;
	    rs->replayed = TRUE;
	    
	    assert(!rs->queued);
	    assert(!rs->completed);
	    
	    //for LSQ entries, don't worry...
	    if((MD_OP_FLAGS(rs->op) & F_LOAD) != F_LOAD){
	      //QUEUE IT TO WAKE UP AT THE RIGHT TIME!!
	      wait_q_enqueue(rs, sim_cycle);
	    }
	    
	    rs = NULL;
	    
	    while(node && !rs){
	      next_node = node->next;
	      if (RSLINK_VALID(node)){
		rs = RSLINK_RS(node);
	      }
	      RSLINK_FREE(node);
	      node = next_node;
	    }
	  }
	  
	  /* FLUSH THE READY QUEUE AS WELL */
	  
	  node = ready_queue;
	  ready_queue = NULL;
	  
	  /* visit all ready instructions (i.e., insts whose register input
	     dependencies have been satisfied, stop issue when no more instructions
	     are available or issue bandwidth is exhausted */
	  for (; node; node = next_node)
	    {
	      next_node = node->next;
	      
	      /* still valid? */
	      if (RSLINK_VALID(node))
		{
		  struct ROB_entry *rs = RSLINK_RS(node);
		  rs->queued = FALSE;
		  rs->replayed = TRUE;
		  if(rs->completed){
		    md_print_insn(rs->IR, rs->PC, stdout);
		    printf("\n");
		  }
		  assert(!rs->completed);
		  
		  if((MD_OP_FLAGS(rs->op) & F_LOAD) != F_LOAD){
		    wait_q_enqueue(rs, sim_cycle);
		  }
		}
	      
	      RSLINK_FREE(node);
	    }
	  
	  break;
	}
	else{
	  assert(FALSE);
	}
      }

      assert(all_operands_ready(rs));

      eventq_queue_event(rs, sim_cycle + rs->exec_lat);


      /* remove from IQ */
      if(rs->in_IQ == 1)
	{
	  /* free up the IQ entry */
	  if(rs->in_IQ){
	    free_iq_entry(rs->iq_entry_num);
	    rs->in_IQ = 0;
	    contexts[rs->context_id].icount--;
	    assert(contexts[rs->context_id].icount >= 0);
	  }
	}
    }
}



/*
 * checks if an instructions operand is marked as ready
 */
static int
operand_ready(struct ROB_entry *rs, int op_num){
  struct reg_set my_regs;
  int ready = TRUE;
  enum reg_type src_type = REG_NONE;

  /* see if it is a floating point or integer register */
  get_reg_set(&my_regs, rs->op);

  if(op_num == 0){
    src_type = my_regs.src1;
  }else{
    src_type = my_regs.src2;
  }

  if(rs->src_physreg[op_num] >= 0){
    switch(src_type){
    case REG_INT:
      ready = (int_reg_file[rs->src_physreg[op_num]].ready <= sim_cycle);
      break;
    case REG_FP:
      ready = (fp_reg_file[rs->src_physreg[op_num]].ready <= sim_cycle);
      break;
    case REG_NONE:
      break;
    }
  }

  return ready;
}

/*
 * checks if an instructions operand is marked as ready (speculative based on load-latency prediction)
 */
static int
operand_spec_ready(struct ROB_entry *rs, int op_num){
  struct reg_set my_regs;
  int ready = TRUE;
  enum reg_type src_type = REG_NONE;

  get_reg_set(&my_regs, rs->op);

  if(op_num == 0){
    src_type = my_regs.src1;
  }else{
    src_type = my_regs.src2;
  }

  if(rs->src_physreg[op_num] >= 0){
    switch(src_type){
    case REG_INT:
      ready = (int_reg_file[rs->src_physreg[op_num]].spec_ready <= sim_cycle);
      break;
    case REG_FP:
      ready = (fp_reg_file[rs->src_physreg[op_num]].spec_ready <= sim_cycle);
      break;
    case REG_NONE:
      break;
    }
  }
  return ready;
}

/*
 * checks if an instruction is ready to issue
 */
static int
all_operands_ready(struct ROB_entry *rs){
  if(rs->ea_comp){
    /* for address computations, we only need to check the second operand (operand 1), which determines the address */
    return operand_ready(rs, 1);
  }
  /* for all other instructions, check both source operands */
  return (operand_ready(rs, 0) && operand_ready(rs, 1));
}

/*
 * checks if an instruction has exactly one ready operand
 */
static int
one_operand_ready(struct ROB_entry *rs){
  return (operand_ready(rs, 0) || operand_ready(rs, 1));
}

/*
 * checks if an instruction is ready to issue (speculative base on load-latency prediction)
 */
static int
all_operands_spec_ready(struct ROB_entry *rs){
  if(rs->ea_comp){
    return operand_spec_ready(rs, 1);
  }
  return (operand_spec_ready(rs, 0) && operand_spec_ready(rs, 1));
}




/*
 * wakeup() - moves instructions from the waiting_q to the ready_q when their
 * source operands become ready
 */
static void
wakeup(void)
{
  
  struct RS_link *node, *next_node;

  for (node = waiting_queue, waiting_queue = NULL;
       node; node = next_node)
    {
      next_node = node->next;

      if (RSLINK_VALID(node)){
	struct ROB_entry *rs = RSLINK_RS(node);
	RSLINK_FREE(node);

	if(all_operands_spec_ready(rs)){
	  /* instruciton ready, so move it to the readyq */
	  assert(!rs->queued);
	  assert(!rs->completed);
	  readyq_enqueue(rs);
	}else{
	  wait_q_enqueue(rs, sim_cycle);
	}
      }else{
	RSLINK_FREE(node);
      }
    }
}

/*
 *  selection() - instruction selection and issue to functional units
 */

/* attempt to issue all operations in the ready queue; insts in the ready
   instruction queue have all register dependencies satisfied, this function
   must then 1) ensure the instructions memory dependencies have been satisfied
   (see lsq_refresh() for details on this process) and 2) a function unit
   is available in this cycle to commence execution of the operation; if all
   goes well, the function unit is allocated, a writeback event is scheduled,
   and the instruction begins execution */
static void
selection(void)
{
  int i, load_lat, tlb_lat, n_issued;
  struct RS_link *node, *next_node;
  struct res_template *fu;

  /* FIXME: could be a little more efficient when scanning the ready queue */

  /* copy and then blow away the ready list, NOTE: the ready list is
     always totally reclaimed each cycle, and instructions that are not
     issue are explicitly reinserted into the ready instruction queue,
     this management strategy ensures that the ready instruction queue
     is always properly sorted */
  node = ready_queue;
  ready_queue = NULL;

  /* visit all ready instructions (i.e., insts whose register input
     dependencies have been satisfied, stop issue when no more instructions
     are available or issue bandwidth is exhausted */
  for (n_issued=0;
       node && n_issued < issue_width;
       node = next_node)
    {
      next_node = node->next;

      /* only issue after the minimum # of cycles has elapsed
       *
       * NOTE: the current implementation assumes that the readyq is sorted
       * by the issue_cycle. If this is changed, then it is not sufficient to
       * simply break here!!
       */

      /* still valid? */
      if (RSLINK_VALID(node))
	{
	  struct ROB_entry *rs = RSLINK_RS(node);
	  struct mem_t* mem = contexts[rs->context_id].mem;

	  /* issue operation, both reg and mem deps have been satisfied */
	  if (!all_operands_spec_ready(rs) || !rs->queued
	      || rs->issued || (!(MD_OP_FLAGS(rs->op) & F_STORE) && rs->completed))
	    panic("issued inst !ready, issued, or completed");

	  /* Wattch -- access window selection logic */
	  window_selection_access++;

	  /* node is now un-queued */
	  rs->queued = FALSE;

	  if (rs->in_LSQ
	      && ((MD_OP_FLAGS(rs->op) & (F_MEM|F_STORE)) == (F_MEM|F_STORE)))
	    {
	      /* stores complete in effectively zero time, result is
		 written into the load/store queue, the actual store into
		 the memory system occurs when the instruction is retired
		 (see commit()) */
	      rs->issued = TRUE;
	      rs->completed = TRUE;
	      
	      if (rs->onames[0] || rs->onames[1])
		panic("store creates result");
	      
	      if (rs->recover_inst)
		panic("mis-predicted store");
	      
	      /* entered execute stage, indicate in pipe trace */
	      ptrace_newstage(rs->ptrace_seq, PST_WRITEBACK, 0);
	      
	      /* one more inst issued */
	      n_issued++;

	      /* Wattch -- LSQ access -- write data into store buffer */
	      lsq_access++;
	      lsq_store_data_access++;
	      lsq_preg_access++;
#ifdef DYNAMIC_AF	
	      lsq_total_pop_count_cycle += pop_count(rs->val_ra);
	      lsq_num_pop_count_cycle++;
#endif

	    }
	  else
	    {
	      /* issue the instruction to a functional unit */
	      if (MD_OP_FUCLASS(rs->op) != NA)
		{
		  fu = res_get(fu_pool, MD_OP_FUCLASS(rs->op));
		  if (fu)
		    {
		      /* got one! issue inst to functional unit */
		      rs->issued = TRUE;

		      /* reserve the functional unit */
		      if (fu->master->busy)
			panic("functional unit already in use");

		      /* schedule functional unit release event */
		      fu->master->busy = fu->issuelat;

		      /* schedule a result writeback event */
		      if (rs->in_LSQ
			  && ((MD_OP_FLAGS(rs->op) & (F_MEM|F_LOAD))
			      == (F_MEM|F_LOAD)))
			{
			  int events = 0;

			  /* Wattch -- LSQ access */
			  lsq_access++;
			  lsq_wakeup_access++;

			  /* for loads, determine cache access latency:
			     first scan LSQ to see if a store forward is
			     possible, if not, access the data cache */
			  load_lat = 0;
			  i = (rs - contexts[rs->context_id].LSQ);
			  if (i != contexts[rs->context_id].LSQ_head)
			    {
			      for (;;)
				{
				  /* go to next earlier LSQ entry */
				  i = (i + (LSQ_size-1)) % LSQ_size;

				  /* FIXME: not dealing with partials! */
				  if ((MD_OP_FLAGS(contexts[rs->context_id].LSQ[i].op) & F_STORE)
				      && (contexts[rs->context_id].LSQ[i].addr == rs->addr))
				    {
				      /* hit in the LSQ */
				      load_lat = 1;
				      break;
				    }

				  /* scan finished? */
				  if (i == contexts[rs->context_id].LSQ_head)
				    break;
				}
			    }

			  /* was the value store forwared from the LSQ? */
			  if (!load_lat)
			    {
			      int valid_addr = MD_VALID_ADDR(rs->addr);

			      if (!rs->spec_mode && !valid_addr)
				sim_invalid_addrs++;

			      /* no! go to the data cache if addr is valid */
			      if (cache_dl1 && valid_addr)
				{
				  /* Wattch -- D-cache access */
				  dcache_access++;

				  /* access the cache if non-faulting */
				  load_lat =
				    cache_access(cache_dl1, Read,
						 (rs->addr & ~3), rs->context_id, NULL, 4,
						 sim_cycle, NULL, NULL);

				  if (load_lat > cache_dl1_lat)
				    events |= PEV_CACHEMISS;
				}
			      else
				{
				  /* no caches defined, just use op latency */
				  load_lat = fu->oplat;
				}
			    }

			  /* all loads and stores must to access D-TLB */
			  if (dtlb && MD_VALID_ADDR(rs->addr))
			    {
			      /* access the D-DLB, NOTE: this code will
				 initiate speculative TLB misses */
			      tlb_lat =
				cache_access(dtlb, Read, (rs->addr & ~3),
					     rs->context_id, NULL, 4, sim_cycle, NULL, NULL);
			      if (tlb_lat > 1)
				events |= PEV_TLBMISS;

			      /* D-cache/D-TLB accesses occur in parallel */
			      load_lat = MAX(tlb_lat, load_lat);
			    }

			  /* use computed cache access latency */
			  rs->exec_lat = load_lat;

			  issue_exec_q_queue_event(rs, sim_cycle + ISSUE_EXEC_DELAY);
			   
			  if(rs->physreg >= 0){
			    int pred_hit_L1 = FALSE;
			    if(!mystricmp(recovery_model, "squash")){
			      /* DO LOAD LATENCY PREDICTION */
			      int stack_recover_idx;
			      struct bpred_update_t dir_update;	/* bpred direction update info */
			      enum md_opcode op = BNE;
			      
			      if(rs->addr){
				pred_hit_L1 =  bpred_lookup(contexts[rs->context_id].load_lat_pred,
							    /* branch address */rs->addr, //CHANGE THIS TO CACHE TARGET???
							    /* target address */ 0,
							    /* opcode */ op,
							    /* call? */ FALSE,
							    /* return? */ FALSE,
							    /* updt */&(dir_update),
							    /* RSB index */&stack_recover_idx);
				assert(rs->addr);
				bpred_update(contexts[rs->context_id].load_lat_pred,
					     /* branch address */rs->addr,
					     /* actual target address */pred_hit_L1,
					     /* taken? */ (rs->exec_lat <= cache_dl1_lat),
					     /* pred taken? */ pred_hit_L1,
					     /* correct pred? */ ((rs->exec_lat <= cache_dl1_lat) == pred_hit_L1),
					     /* opcode */op,
					     /* predictor update ptr */&(dir_update));
			      }else{
				pred_hit_L1 = 0;
			      }
			    }
			    
			    /* wakeup dependents */
			    switch(rs->dest_format){
			    case REG_INT:
			      assert(rs->physreg >= 0);
			      /* LOAD PREDICTION HERE!!!!! */
			      /* earliest cycle dependents should wakeup */
			      if(!mystricmp(recovery_model, "squash")){
				int_reg_file[rs->physreg].spec_ready = sim_cycle + (pred_hit_L1 ? cache_dl1_lat : (rs->exec_lat + ISSUE_EXEC_DELAY));
			      }else{
				int_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
			      }
			      /* data will be on the bypass network when this instruction completes execution */
			      int_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
			      break;
			      
			    case REG_FP:
			      assert(rs->physreg >= 0);
			      /* LOAD PREDICTION HERE!!!!! */
			      /* earliest cycle dependents should wakeup */
			      if(!mystricmp(recovery_model, "squash")){
				fp_reg_file[rs->physreg].spec_ready = sim_cycle + (pred_hit_L1 ? cache_dl1_lat : (rs->exec_lat + ISSUE_EXEC_DELAY));
			      }else {
				fp_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
			      }
			      /* data will be on the bypass network when this instruction completes execution */
			      fp_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
			      break;
			      
			    case REG_NONE:
			      break;
			    };
			  }

			  /* entered execute stage, indicate in pipe trace */
			  ptrace_newstage(rs->ptrace_seq, PST_EXECUTE,
					  ((rs->ea_comp ? PEV_AGEN : 0)
					   | events));
			}
		      else /* !load && !store */
			{
			  /* Wattch -- ALU access Wattch-FIXME 
			     (different op types) 
			     also spread out power of multi-cycle ops 
			  */
			  alu_access++;

			  if((MD_OP_FLAGS(rs->op) & (F_FCOMP))== (F_FCOMP))
			    falu_access++;
			  else
			    ialu_access++;
			  
			  /* use deterministic functional unit latency */
			  rs->exec_lat = fu->oplat;
			  issue_exec_q_queue_event(rs, sim_cycle + ISSUE_EXEC_DELAY);
			
			  //memory accesses are woken up differently via the LSQ
			  if((rs->physreg >= 0) && (!rs->ea_comp)){
			    /* wakeup dependents */
			    switch(rs->dest_format){
			    case REG_INT:
			      assert(rs->physreg >= 0);
			      /* earliest cycle dependents should wakeup */
			      int_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
			      /* data will be on the bypass network when this instruction completes execution */
			      int_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
			      break;
			      
			    case REG_FP:
			      assert(rs->physreg >= 0);
			      /* earliest cycle dependents should wakeup */
			      fp_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
			      /* data will be on the bypass network when this instruction completes execution */
			      fp_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
			      break;
			      
			    case REG_NONE:
			      break;
			    };
			  }

			  /* entered execute stage, indicate in pipe trace */
			  ptrace_newstage(rs->ptrace_seq, PST_EXECUTE, 
					  rs->ea_comp ? PEV_AGEN : 0);
			}

		      /* Wattch -- window access */
		      window_access++;
		      /* read values from window send to FUs */
		      window_preg_access++;
		      window_preg_access++;
#ifdef DYNAMIC_AF	
		      window_total_pop_count_cycle += pop_count(rs->val_ra) + pop_count(rs->val_rb);
		      window_num_pop_count_cycle+=2;
#endif
		      
		      /* one more inst issued */
		      n_issued++;
		    }
		  else /* no functional unit */
		    {
		      /* insufficient functional unit resources, put operation
			 back onto the ready list, we'll try to issue it
			 again next cycle */
		      readyq_enqueue(rs);
		    }
		}
	      else /* does not require a functional unit! */
		{
		  /* FIXME: need better solution for these */
		  /* the instruction does not need a functional unit */
		  rs->issued = TRUE;

		  /* schedule a result event */
		  rs->exec_lat = 1;
		  issue_exec_q_queue_event(rs, sim_cycle + ISSUE_EXEC_DELAY);
		
		  /* wakeup dependents */
		  switch(rs->dest_format){
		  case REG_INT:
		    assert(rs->physreg >= 0);
		    /* earliest cycle dependents should wakeup */
		    int_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
		    /* data will be on the bypass network when this instruction completes execution */
		    int_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
		    break;
		    
		  case REG_FP:
		    assert(rs->physreg >= 0);
		    /* earliest cycle dependents should wakeup */
		    fp_reg_file[rs->physreg].spec_ready = sim_cycle + rs->exec_lat;
		    /* data will be on the bypass network when this instruction completes execution */
		    fp_reg_file[rs->physreg].ready = sim_cycle + rs->exec_lat + ISSUE_EXEC_DELAY;
		    break;
		    
		  case REG_NONE:
		    break;
		  };
		  
		  
		  /* entered execute stage, indicate in pipe trace */
		  ptrace_newstage(rs->ptrace_seq, PST_EXECUTE,
				  rs->ea_comp ? PEV_AGEN : 0);


		  /* Wattch -- Window access */
		  window_access++;
		  /* read values from window send to FUs */
		  window_preg_access++;
		  window_preg_access++;
#ifdef DYNAMIC_AF	
		  window_total_pop_count_cycle += pop_count(rs->val_ra) + pop_count(rs->val_rb);
		  window_num_pop_count_cycle+=2;
#endif
	      
		  /* one more inst issued */
		  n_issued++;
		}
	    } /* !store */

	}
      /* else, ROB entry was squashed */

      /* reclaim ready list entry, NOTE: this is done whether or not the
         instruction issued, since the instruction was once again reinserted
         into the ready queue if it did not issue, this ensures that the ready
         queue is always properly sorted */
      RSLINK_FREE(node);
    }

  /* put any instruction not issued back into the ready queue, go through
     normal channels to ensure instruction stay ordered correctly */
  for (; node; node = next_node)
    {
      next_node = node->next;

      /* still valid? */
      if (RSLINK_VALID(node))
        {
	  struct ROB_entry *rs = RSLINK_RS(node);

          /* node is now un-queued */
          rs->queued = FALSE;

	  /* not issued, put operation back onto the ready list, we'll try to
	     issue it again next cycle */
          readyq_enqueue(rs);
        }
      /* else, ROB entry was squashed */

      /* reclaim ready list entry, NOTE: this is done whether or not the
         instruction issued, since the instruction was once again reinserted
         into the ready queue if it did not issue, this ensures that the ready
         queue is always properly sorted */
      RSLINK_FREE(node);
    }
}


/*
 * routines for generating on-the-fly instruction traces with support
 * for control and data misspeculation modeling
 */

/* integer register file */
#define R_BMAP_SZ       (BITMAP_SIZE(MD_NUM_IREGS))

/* floating point register file */
#define F_BMAP_SZ       (BITMAP_SIZE(MD_NUM_FREGS))

/* miscellaneous registers */
#define C_BMAP_SZ       (BITMAP_SIZE(MD_NUM_CREGS))

/* dump speculative register state */
static void
rspec_dump(FILE *stream)			/* output stream */
{
  int i;
  struct regs_t* spec_regs = &contexts[0].spec_regs;
  int spec_mode = contexts[0].spec_mode;
  

  if (!stream)
    stream = stderr;

  fprintf(stream, "** speculative register contents **\n");

  fprintf(stream, "spec_mode: %s\n", spec_mode ? "t" : "f");

  /* dump speculative integer regs */
  for (i=0; i < MD_NUM_IREGS; i++)
    {
      if (BITMAP_SET_P(contexts[0].use_spec_R, R_BMAP_SZ, i))
	{
	  md_print_ireg(spec_regs->regs_R, i, stream);
	  fprintf(stream, "\n");
	}
    }

  /* dump speculative FP regs */
  for (i=0; i < MD_NUM_FREGS; i++)
    {
      if (BITMAP_SET_P(contexts[0].use_spec_F, F_BMAP_SZ, i))
	{
	  md_print_fpreg(spec_regs->regs_F, i, stream);
	  fprintf(stream, "\n");
	}
    }

  /* dump speculative CTRL regs */
  for (i=0; i < MD_NUM_CREGS; i++)
    {
      if (BITMAP_SET_P(contexts[0].use_spec_C, C_BMAP_SZ, i))
	{
	  md_print_creg(spec_regs->regs_C, i, stream);
	  fprintf(stream, "\n");
	}
    }
}


/* speculative memory hash table size, NOTE: this must be a power-of-two */
#define STORE_HASH_SIZE		32

/* speculative memory hash table definition, accesses go through this hash
   table when accessing memory in speculative mode, the hash table flush the
   table when recovering from mispredicted branches */
struct spec_mem_ent {
  struct spec_mem_ent *next;		/* ptr to next hash table bucket */
  md_addr_t addr;			/* virtual address of spec state */
  unsigned int data[2];			/* spec buffer, up to 8 bytes */
};

/* speculative memory hash table */
static struct spec_mem_ent *store_htable[STORE_HASH_SIZE];

/* speculative memory hash table bucket free list */
static struct spec_mem_ent *bucket_free_list = NULL;


/* IFETCH -> RENAME instruction queue definition */
struct fetch_rec {
  md_inst_t IR;				/* inst register */
  md_addr_t regs_PC, pred_PC;		/* current PC, predicted next PC */
  struct bpred_update_t dir_update;	/* bpred direction update info */
  int stack_recover_idx;		/* branch predictor RSB index */
  unsigned int ptrace_seq;		/* print trace sequence id */
  tick_t fetched_cycle;                 /* cycle in which the instruction was fetched */
};

/* recover instruction trace generator state to precise state state immediately
   before the first mis-predicted branch; this is accomplished by resetting
   all register value copied-on-write bitmasks are reset, and the speculative
   memory hash table is cleared */
static void
tracer_recover(int context_id)
{
  int i;
  struct spec_mem_ent *ent, *ent_next;

  /* better be in mis-speculative trace generation mode */
  if (!contexts[context_id].spec_mode)
    panic("cannot recover unless in speculative mode");

  /* reset to non-speculative trace generation mode */
  contexts[context_id].spec_mode = FALSE;

  /* reset copied-on-write register bitmasks back to non-speculative state */
  BITMAP_CLEAR_MAP(contexts[context_id].use_spec_R, R_BMAP_SZ);
  BITMAP_CLEAR_MAP(contexts[context_id].use_spec_F, F_BMAP_SZ);
  BITMAP_CLEAR_MAP(contexts[context_id].use_spec_C, C_BMAP_SZ);

  /* reset memory state back to non-speculative state */
  /* FIXME: could version stamps be used here?!?!? */
  for (i=0; i<STORE_HASH_SIZE; i++)
    {
      /* release all hash table buckets */
      for (ent=store_htable[i]; ent; ent=ent_next)
	{
	  ent_next = ent->next;
	  ent->next = bucket_free_list;
	  bucket_free_list = ent;
	}
      store_htable[i] = NULL;
    }

  
  contexts[context_id].icount -= contexts[context_id].fetch_num;

  /* if pipetracing, indicate squash of instructions in the inst fetch queue */
  if (ptrace_active)
    {
      while (contexts[context_id].fetch_num != 0)
	{
	  /* squash the next instruction from the IFETCH -> RENAME queue */
	  ptrace_endinst(contexts[context_id].fetch_data[contexts[context_id].fetch_head].ptrace_seq);

	  /* consume instruction from IFETCH -> RENAME queue */
	  contexts[context_id].fetch_head = (contexts[context_id].fetch_head+1) & (ifq_size - 1);
	  contexts[context_id].fetch_num--;
	}
    }

  /* reset IFETCH state */
  contexts[context_id].fetch_num = 0;
  contexts[context_id].fetch_tail = contexts[context_id].fetch_head = 0;
  
  contexts[context_id].fetch_pred_PC = contexts[context_id].fetch_regs_PC = contexts[context_id].recover_PC;
}

/* initialize the speculative instruction state generator state */
static void
tracer_init(int context_id)
{
  int i;

  /* initially in non-speculative mode */
  contexts[context_id].spec_mode = FALSE;

  /* register state is from non-speculative state buffers */
  BITMAP_CLEAR_MAP(contexts[context_id].use_spec_R, R_BMAP_SZ);
  BITMAP_CLEAR_MAP(contexts[context_id].use_spec_F, F_BMAP_SZ);

  /* memory state is from non-speculative memory pages */
  for (i=0; i<STORE_HASH_SIZE; i++)
    store_htable[i] = NULL;
}


/* speculative memory hash table address hash function */
#define HASH_ADDR(ADDR)							\
  ((((ADDR) >> 24)^((ADDR) >> 16)^((ADDR) >> 8)^(ADDR)) & (STORE_HASH_SIZE-1))

/* this functional provides a layer of mis-speculated state over the
   non-speculative memory state, when in mis-speculation trace generation mode,
   the simulator will call this function to access memory, instead of the
   non-speculative memory access interfaces defined in memory.h; when storage
   is written, an entry is allocated in the speculative memory hash table,
   future reads and writes while in mis-speculative trace generation mode will
   access this buffer instead of non-speculative memory state; when the trace
   generator transitions back to non-speculative trace generation mode,
   tracer_recover() clears this table, returns any access fault */
static enum md_fault_type
spec_mem_access(struct mem_t *mem,		/* memory space to access */
		enum mem_cmd cmd,		/* Read or Write access cmd */
		md_addr_t addr,			/* virtual address of access */
		void *p,			/* input/output buffer */
		int nbytes)			/* number of bytes to access */
{
  int i, index;
  struct spec_mem_ent *ent, *prev;

  /* FIXME: partially overlapping writes are not combined... */
  /* FIXME: partially overlapping reads are not handled correctly... */

  /* check alignments, even speculative this test should always pass */
  if ((nbytes & (nbytes-1)) != 0 || (addr & (nbytes-1)) != 0)
    {
      /* no can do, return zero result */
      for (i=0; i < nbytes; i++)
	((char *)p)[i] = 0;

      return md_fault_none;
    }

  /* check permissions */
  if (!((addr >= mem->ld_text_base && addr < (mem->ld_text_base+mem->ld_text_size)
	 && cmd == Read)
	|| MD_VALID_ADDR(addr)))
    {
      /* no can do, return zero result */
      for (i=0; i < nbytes; i++)
	((char *)p)[i] = 0;

      return md_fault_none;
    }

  /* has this memory state been copied on mis-speculative write? */
  index = HASH_ADDR(addr);
  for (prev=NULL,ent=store_htable[index]; ent; prev=ent,ent=ent->next)
    {
      if (ent->addr == addr)
	{
	  /* reorder chains to speed access into hash table */
	  if (prev != NULL)
	    {
	      /* not at head of list, relink the hash table entry at front */
	      prev->next = ent->next;
              ent->next = store_htable[index];
              store_htable[index] = ent;
	    }
	  break;
	}
    }

  /* no, if it is a write, allocate a hash table entry to hold the data */
  if (!ent && cmd == Write)
    {
      /* try to get an entry from the free list, if available */
      if (!bucket_free_list)
	{
	  /* otherwise, call calloc() to get the needed storage */
	  bucket_free_list = calloc(1, sizeof(struct spec_mem_ent));
	  if (!bucket_free_list)
	    fatal("out of virtual memory");
	}
      ent = bucket_free_list;
      bucket_free_list = bucket_free_list->next;

      /* insert into hash table */
      ent->next = store_htable[index];
      store_htable[index] = ent;
      ent->addr = addr;
      ent->data[0] = 0; ent->data[1] = 0;
    }
  
  /* handle the read or write to speculative or non-speculative storage */
  switch (nbytes)
    {
    case 1:
      if (cmd == Read)
	{
	  if (ent)
	    {
	      /* read from mis-speculated state buffer */
	      *((byte_t *)p) = *((byte_t *)(&ent->data[0]));
	    }
	  else
	    {
	      /* read from non-speculative memory state, don't allocate
	         memory pages with speculative loads */
	      *((byte_t *)p) = MEM_READ_BYTE(mem, addr);
	    }
	}
      else
	{
	  /* always write into mis-speculated state buffer */
	  *((byte_t *)(&ent->data[0])) = *((byte_t *)p);
	}
      break;
    case 2:
      if (cmd == Read)
	{
	  if (ent)
	    {
	      /* read from mis-speculated state buffer */
	      *((half_t *)p) = *((half_t *)(&ent->data[0]));
	    }
	  else
	    {
	      /* read from non-speculative memory state, don't allocate
	         memory pages with speculative loads */
	      *((half_t *)p) = MEM_READ_HALF(mem, addr);
	    }
	}
      else
	{
	  /* always write into mis-speculated state buffer */
	  *((half_t *)&ent->data[0]) = *((half_t *)p);
	}
      break;
    case 4:
      if (cmd == Read)
	{
	  if (ent)
	    {
	      /* read from mis-speculated state buffer */
	      *((word_t *)p) = *((word_t *)&ent->data[0]);
	    }
	  else
	    {
	      /* read from non-speculative memory state, don't allocate
	         memory pages with speculative loads */
	      *((word_t *)p) = MEM_READ_WORD(mem, addr);
	    }
	}
      else
	{
	  /* always write into mis-speculated state buffer */
	  *((word_t *)&ent->data[0]) = *((word_t *)p);
	}
      break;
    case 8:
      if (cmd == Read)
	{
	  if (ent)
	    {
	      /* read from mis-speculated state buffer */
	      *((word_t *)p) = *((word_t *)&ent->data[0]);
	      *(((word_t *)p)+1) = *((word_t *)&ent->data[1]);
	    }
	  else
	    {
	      /* read from non-speculative memory state, don't allocate
	         memory pages with speculative loads */
	      *((word_t *)p) = MEM_READ_WORD(mem, addr);
	      *(((word_t *)p)+1) =
		MEM_READ_WORD(mem, addr + sizeof(word_t));
	    }
	}
      else
	{
	  /* always write into mis-speculated state buffer */
	  *((word_t *)&ent->data[0]) = *((word_t *)p);
	  *((word_t *)&ent->data[1]) = *(((word_t *)p)+1);
	}
      break;
    default:
      panic("access size not supported in mis-speculative mode");
    }

  return md_fault_none;
}

/* dump speculative memory state */
static void
mspec_dump(FILE *stream, int context_id)	     	/* output stream */
{
  int i;
  struct spec_mem_ent *ent;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** speculative memory contents **\n");

  fprintf(stream, "spec_mode: %s\n", contexts[context_id].spec_mode ? "t" : "f");

  for (i=0; i<STORE_HASH_SIZE; i++)
    {
      /* dump contents of all hash table buckets */
      for (ent=store_htable[i]; ent; ent=ent->next)
	{
	  myfprintf(stream, "[0x%08p]: %12.0f/0x%08x:%08x\n",
		    ent->addr, (double)(*((double *)ent->data)),
		    *((unsigned int *)&ent->data[0]),
		    *(((unsigned int *)&ent->data[0]) + 1));
	}
    }
}

/* default memory state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_mem_obj(struct mem_t *mem,		/* memory space to access */
	      int is_write,			/* access type */
	      md_addr_t addr,			/* address to access */
	      char *p,				/* input/output buffer */
	      int nbytes)			/* size of access */
{
  enum mem_cmd cmd;

  if (!is_write)
    cmd = Read;
  else
    cmd = Write;

#if 0
  char *errstr;

  errstr = mem_valid(cmd, addr, nbytes, /* declare */FALSE);
  if (errstr)
    return errstr;
#endif

  /* else, no error, access memory */
  if (contexts[mem->context_id].spec_mode)
    spec_mem_access(mem, cmd, addr, p, nbytes);
  else
    mem_access(mem, cmd, addr, p, nbytes);

  /* no error */
  return NULL;
}

/*
 *  RENAME() - decode instructions, rename the registers, and allocate ROB and LSQ resources
 */

/*
 * configure the instruction decode engine
 */

#define DNA			(0)

/* general register dependence decoders, $r31 maps to DNA (0) */
#define DGPR(N)			(31 - (N)) /* was: (((N) == 31) ? DNA : (N)) */

/* floating point register dependence decoders */
#define DFPR(N)			(((N) == 31) ? DNA : ((N)+32))

/* miscellaneous register dependence decoders */
#define DFPCR			(0+32+32)
#define DUNIQ			(1+32+32)
#define DTMP			(2+32+32)

/*
 * configure the execution engine
 */

/* next program counter */
#define SET_NPC(EXPR)           (regs->regs_NPC = (EXPR))

/* target program counter */
#undef  SET_TPC
#define SET_TPC(EXPR)		(target_PC = (EXPR))

/* current program counter */
#define CPC                     (regs->regs_PC)
#define SET_CPC(EXPR)           (regs->regs_PC = (EXPR))

/* general purpose register accessors, NOTE: speculative copy on write storage
   provided for fast recovery during wrong path execute (see tracer_recover()
   for details on this process */
#define GPR(N)                  (BITMAP_SET_P(contexts[bitmap_context_id].use_spec_R, R_BMAP_SZ, (N))\
				 ? spec_regs->regs_R[N]                       \
				 : regs->regs_R[N])
#define SET_GPR(N,EXPR)         (spec_mode				\
				 ? ((spec_regs->regs_R[N] = (EXPR)),		\
				    BITMAP_SET(contexts[bitmap_context_id].use_spec_R, R_BMAP_SZ, (N))\
				    /*spec_regs->regs_R[N]*/)			\
				 : (regs->regs_R[N] = (EXPR)))

/* floating point register accessors, NOTE: speculative copy on write storage
   provided for fast recovery during wrong path execute (see tracer_recover()
   for details on this process */
#define FPR_Q(N)		(BITMAP_SET_P(contexts[bitmap_context_id].use_spec_F, F_BMAP_SZ, (N))\
				 ? spec_regs->regs_F.q[(N)]                   \
				 : regs->regs_F.q[(N)])
#define SET_FPR_Q(N,EXPR)	(spec_mode				\
				 ? ((spec_regs->regs_F.q[(N)] = (EXPR)),	\
				    BITMAP_SET(contexts[bitmap_context_id].use_spec_F,F_BMAP_SZ, (N)),\
				    spec_regs->regs_F.q[(N)])			\
				 : (regs->regs_F.q[(N)] = (EXPR)))
#define FPR(N)			(BITMAP_SET_P(contexts[bitmap_context_id].use_spec_F, F_BMAP_SZ, (N))\
				 ? spec_regs->regs_F.d[(N)]			\
				 : regs->regs_F.d[(N)])
#define SET_FPR(N,EXPR)		(spec_mode				\
				 ? ((spec_regs->regs_F.d[(N)] = (EXPR)),	\
				    BITMAP_SET(contexts[bitmap_context_id].use_spec_F,F_BMAP_SZ, (N)),\
				    spec_regs->regs_F.d[(N)])			\
				 : (regs->regs_F.d[(N)] = (EXPR)))

/* miscellanous register accessors, NOTE: speculative copy on write storage
   provided for fast recovery during wrong path execute (see tracer_recover()
   for details on this process */
#define FPCR			(BITMAP_SET_P(contexts[bitmap_context_id].use_spec_C, C_BMAP_SZ,/*fpcr*/0)\
				 ? spec_regs->regs_C.fpcr			\
				 : regs->regs_C.fpcr)
#define SET_FPCR(EXPR)		(spec_mode				\
				 ? ((spec_regs->regs_C.fpcr = (EXPR)),	\
				   BITMAP_SET(contexts[bitmap_context_id].use_spec_C,C_BMAP_SZ,/*fpcr*/0),\
				    spec_regs->regs_C.fpcr)			\
				 : (regs->regs_C.fpcr = (EXPR)))
#define UNIQ			(BITMAP_SET_P(contexts[bitmap_context_id].use_spec_C, C_BMAP_SZ,/*uniq*/1)\
				 ? spec_regs->regs_C.uniq			\
				 : regs->regs_C.uniq)
#define SET_UNIQ(EXPR)		(spec_mode				\
				 ? ((spec_regs->regs_C.uniq = (EXPR)),	\
				   BITMAP_SET(contexts[bitmap_context_id].use_spec_C,C_BMAP_SZ,/*uniq*/1),\
				    spec_regs->regs_C.uniq)			\
				 : (regs->regs_C.uniq = (EXPR)))
#define FCC			(BITMAP_SET_P(contexts[bitmap_context_id].use_spec_C, C_BMAP_SZ,/*fcc*/2)\
				 ? spec_regs->regs_C.fcc			\
				 : regs->regs_C.fcc)
#define SET_FCC(EXPR)		(spec_mode				\
				 ? ((spec_regs->regs_C.fcc = (EXPR)),		\
				    BITMAP_SET(contexts[bitmap_context_id].use_spec_C,C_BMAP_SZ,/*fcc*/1),\
				    spec_regs->regs_C.fcc)			\
				 : (regs->regs_C.fcc = (EXPR)))

/* precise architected memory state accessor macros, NOTE: speculative copy on
   write storage provided for fast recovery during wrong path execute (see
   tracer_recover() for details on this process */
#define __READ_SPECMEM(SRC, SRC_V, FAULT)				\
  (addr = (SRC),							\
   (spec_mode								\
    ? ((FAULT) = spec_mem_access(mem, Read, addr, &SRC_V, sizeof(SRC_V)))\
    : ((FAULT) = mem_access(mem, Read, addr, &SRC_V, sizeof(SRC_V)))),	\
   SRC_V)

#define READ_BYTE(SRC, FAULT)						\
  __READ_SPECMEM((SRC), temp_byte, (FAULT))
#define READ_HALF(SRC, FAULT)						\
  MD_SWAPH(__READ_SPECMEM((SRC), temp_half, (FAULT)))
#define READ_WORD(SRC, FAULT)						\
  MD_SWAPW(__READ_SPECMEM((SRC), temp_word, (FAULT)))
#ifdef HOST_HAS_QWORD
#define READ_QWORD(SRC, FAULT)						\
  MD_SWAPQ(__READ_SPECMEM((SRC), temp_qword, (FAULT)))
#endif /* HOST_HAS_QWORD */


#define __WRITE_SPECMEM(SRC, DST, DST_V, FAULT)				\
  (DST_V = (SRC), addr = (DST),						\
   (spec_mode								\
    ? ((FAULT) = spec_mem_access(mem, Write, addr, &DST_V, sizeof(DST_V)))\
    : ((FAULT) = mem_access(mem, Write, addr, &DST_V, sizeof(DST_V)))))

#define WRITE_BYTE(SRC, DST, FAULT)					\
  __WRITE_SPECMEM((SRC), (DST), temp_byte, (FAULT))
#define WRITE_HALF(SRC, DST, FAULT)					\
  __WRITE_SPECMEM(MD_SWAPH(SRC), (DST), temp_half, (FAULT))
#define WRITE_WORD(SRC, DST, FAULT)					\
  __WRITE_SPECMEM(MD_SWAPW(SRC), (DST), temp_word, (FAULT))
#ifdef HOST_HAS_QWORD
#define WRITE_QWORD(SRC, DST, FAULT)					\
  __WRITE_SPECMEM(MD_SWAPQ(SRC), (DST), temp_qword, (FAULT))
#endif /* HOST_HAS_QWORD */

/* system call handler macro */
#define SYSCALL(INST)							\
  (/* only execute system calls in non-speculative mode */		\
   (spec_mode ? panic("speculative syscall") : (void) 0),		\
   sys_syscall(regs, mem_access, mem, INST, TRUE))

/* default register state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_reg_obj(struct regs_t *regs,		/* registers to access */
	      int is_write,			/* access type */
	      enum md_reg_type rt,		/* reg bank to probe */
	      int reg,				/* register number */
	      struct eval_value_t *val)		/* input, output */
{

  struct regs_t* spec_regs = &contexts[regs->context_id].spec_regs;
  int spec_mode = contexts[regs->context_id].spec_mode;
  int bitmap_context_id = regs->context_id;

  switch (rt)
    {
    case rt_gpr:
      if (reg < 0 || reg >= MD_NUM_IREGS)
	return "register number out of range";

      if (!is_write)
	{
	  val->type = et_uint;
	  val->value.as_uint = GPR(reg);
	}
      else
	SET_GPR(reg, eval_as_uint(*val));
      break;

    case rt_lpr:
      if (reg < 0 || reg >= MD_NUM_FREGS)
	return "register number out of range";

      /* FIXME: this is not portable... */
      abort();
#if 0
      if (!is_write)
	{
	  val->type = et_uint;
	  val->value.as_uint = FPR_L(reg);
	}
      else
	SET_FPR_L(reg, eval_as_uint(*val));
#endif
      break;

    case rt_fpr:
      /* FIXME: this is not portable... */
      abort();
#if 0
      if (!is_write)
	val->value.as_float = FPR_F(reg);
      else
	SET_FPR_F(reg, val->value.as_float);
#endif
      break;

    case rt_dpr:
      /* FIXME: this is not portable... */
      abort();
#if 0
      /* 1/2 as many regs in this mode */
      if (reg < 0 || reg >= MD_NUM_REGS/2)
	return "register number out of range";

      if (at == at_read)
	val->as_double = FPR_D(reg * 2);
      else
	SET_FPR_D(reg * 2, val->as_double);
#endif
      break;

      /* FIXME: this is not portable... */
#if 0
      abort();
    case rt_hi:
      if (at == at_read)
	val->as_word = HI;
      else
	SET_HI(val->as_word);
      break;
    case rt_lo:
      if (at == at_read)
	val->as_word = LO;
      else
	SET_LO(val->as_word);
      break;
    case rt_FCC:
      if (at == at_read)
	val->as_condition = FCC;
      else
	SET_FCC(val->as_condition);
      break;
#endif

    case rt_PC:
      if (!is_write)
	{
	  val->type = et_addr;
	  val->value.as_addr = regs->regs_PC;
	}
      else
	regs->regs_PC = eval_as_addr(*val);
      break;

    case rt_NPC:
      if (!is_write)
	{
	  val->type = et_addr;
	  val->value.as_addr = regs->regs_NPC;
	}
      else{
	printf("eval as addr...\n");
	regs->regs_NPC = eval_as_addr(*val);
      }
      break;

    default:
      panic("bogus register bank");
    }

  /* no error */
  return NULL;
}

/* the last operation that rename() attempted to rename, for
   implementing in-order issue */
static struct RS_link last_op = RSLINK_NULL_DATA;

/*
 * Returns the set of register types used by a specific operation
 */
static void get_reg_set(struct reg_set* my_regs, enum md_opcode op){
  my_regs->src1 = REG_NONE;
  my_regs->src2 = REG_NONE;
  my_regs->dest = REG_NONE;
  my_regs->load = 0;
  my_regs->store = 0;

  switch(op){
  case LDA:
  case LDAH:
  case LDBU:
  case LDQ_U:
  case LDWU:
  case LDL:
  case LDQ:
  case LDL_L:
  case LDQ_L:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_INT;
    my_regs->dest = REG_INT;
    my_regs->load = 1;
    break;

  case STW:
  case STB:
  case STQ_U:
  case STL:
  case STQ:
  case STL_C:
  case STQ_C:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_INT;
    my_regs->dest = REG_NONE;
    my_regs->store = 1;
    break;

  case FLTV:
  case LDG:
  case STF:
  case STG:
  case PAL_CALLSYS:
  case SQRTF:
  case ITOFF:
  case SQRTG:
  case TRAPB:
  case EXCB:
  case MB:
  case WMB:
  case FETCH:
  case FETCH_M:
  case _RC:
  case ECB:
  case _RS:
  case WH64:
  case OP_NA:
  case CALL_PAL:
  case LDF:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_NONE;
    break;

  case LDS:
  case LDT:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_INT;
    my_regs->dest = REG_FP;
    my_regs->load = 1;
    break;
    
  case STS:
  case STT:
    my_regs->src1 = REG_FP;
    my_regs->src2 = REG_INT;
    my_regs->dest = REG_NONE;
    break;

  case BR:
  case BSR:
  case IMPLVER:
  case RPCC:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_INT;
    break;

  case FBEQ:
  case FBLT:
  case FBLE:
  case FBNE:
  case FBGE:
  case FBGT:
    my_regs->src1 = REG_FP;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_NONE;
    break;

  case BLBC:
  case BEQ:
  case BLT:
  case BLE:
  case BLBS:
  case BNE:
  case BGE:
  case BGT:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_NONE;
    break;

  case ADDL:
  case S4ADDL:
  case SUBL:
  case S4SUBL:
  case CMPBGE:
  case S8ADDL:
  case S8SUBL:
  case CMPULT:
  case ADDQ:
  case S4ADDQ:
  case SUBQ:
  case S4SUBQ:
  case CMPEQ:
  case S8ADDQ:
  case S8SUBQ:
  case CMPULE:
  case ADDLV:
  case SUBLV:
  case CMPLT:
  case ADDQV:
  case SUBQV:
  case CMPLE:
  case AND:
  case BIC:
  case CMOVLBS:
  case CMOVLBC:
  case BIS:
  case CMOVEQ:
  case CMOVNE:
  case ORNOT:
  case XOR:
  case CMOVLT:
  case CMOVGE:
  case EQV:
  case CMOVLE:
  case CMOVGT:
  case MSKBL:
  case EXTBL:
  case INSBL:
  case MSKWL:
  case EXTWL:
  case INSWL:
  case MSKLL:
  case EXTLL:
  case INSLL:
  case ZAP:
  case ZAPNOT:
  case MSKQL:
  case SRL:
  case EXTQL:
  case SLL:
  case INSQL:
  case SRA:
  case MSKWH:
  case INSWH:
  case EXTWH:
  case MSKLH:
  case INSLH:
  case EXTLH:
  case MSKQH:
  case INSQH:
  case EXTQH:
  case MULL:
  case MULQ:
  case UMULH:
  case PERR:
  case MINSB8:
  case MINSW4:
  case MINUB8:
  case MINUW4:
  case MAXUB8:
  case MAXUW4:
  case MAXSB8:
  case MAXSW4:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_INT;
    my_regs->dest = REG_INT;
    break;

  case ADDLI:
  case S4ADDLI:
  case SUBLI:
  case S4SUBLI:
  case CMPBGEI:
  case S8ADDLI:
  case S8SUBLI:
  case CMPULTI:
  case ADDQI:
  case S4ADDQI:
  case SUBQI:
  case S4SUBQI:
  case CMPEQI:
  case S8ADDQI:
  case S8SUBQI:
  case CMPULEI:
  case ADDLVI:
  case SUBLVI:
  case CMPLTI:
  case ADDQVI:
  case SUBQVI:
  case CMPLEI:
  case ANDI:
  case BICI:
  case CMOVLBSI:
  case CMOVLBCI:
  case BISI:
  case CMOVEQI:
  case CMOVNEI:
  case ORNOTI:
  case XORI:
  case CMOVLTI:
  case CMOVGEI:
  case EQVI:
  case AMASK:
  case CMOVLEI:
  case CMOVGTI:
  case MSKBLI:
  case EXTBLI:
  case INSBLI:
  case MSKWLI:
  case EXTWLI:
  case INSWLI:
  case MSKLLI:
  case EXTLLI:
  case INSLLI:
  case ZAPI:
  case ZAPNOTI:
  case MSKQLI:
  case SRLI:
  case EXTQLI:
  case SLLI:
  case INSQLI:
  case SRAI:
  case MSKWHI:
  case INSWHI:
  case EXTWHI:
  case MSKLHI:
  case INSLHI:
  case EXTLHI:
  case MSKQHI:
  case INSQHI:
  case EXTQHI:
  case MULLI:
  case MULQI:
  case UMULHI:
  case JMP:
  case JSR:
  case RETN:
  case JSR_COROUTINE:
  case CTPOP:
  case CTLZ:
  case CTTZ:
  case UNPKBW:
  case UNPKBL:
  case PKWB:
  case PKLB:
  case MINSB8I:
  case MINSW4I:
  case MINUB8I:
  case MINUW4I:
  case MAXUB8I:
  case MAXUW4I:
  case MAXSB8I:
  case MAXSW4I:
  case SEXTB:
  case SEXTW:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_INT;
    break;

  case AMASKI:
  case SEXTBI:
  case SEXTWI:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_INT;
    break;

  case ITOFS:
  case ITOFT:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_FP;
    break;

  case SQRTS:
  case SQRTT:
  case CVTTS:
  case CVTTQ:
  case CVTQS:
  case CVTQT:
  case CVTLQ:
  case CVTQL:
    my_regs->src1 = REG_FP;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_FP;
    break;


  case ADDS:
  case SUBS:
  case MULS:
  case DIVS:
  case ADDT:
  case SUBT:
  case MULT:
  case DIVT:
  case CMPTUN:
  case CMPTEQ:
  case CMPTLT:
  case CMPTLE:
  case CPYS:
  case CPYSN:
  case CPYSE:
  case FCMOVEQ:
  case FCMOVNE:
  case FCMOVLT:
  case FCMOVGE:
  case FCMOVLE:
  case FCMOVGT:
    my_regs->src1 = REG_FP;
    my_regs->src2 = REG_FP;
    my_regs->dest = REG_FP;
    break;


  case FTOIT:
  case FTOIS:
    my_regs->src1 = REG_FP;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_INT;
    break;

  case MT_FPCR:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_FP;
    break;


  case PAL_RDUNIQ:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_INT;
    break;
  case PAL_WRUNIQ:
    my_regs->src1 = REG_INT;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_NONE;
    break;

  default:
    my_regs->src1 = REG_NONE;
    my_regs->src2 = REG_NONE;
    my_regs->dest = REG_NONE;
    break;

  }

}


/* counts the number of instructions in the rename->dispatch pipeline for a particular thread */
static int
not_dispatched_count(int c_id){
  int count = 0;
  int ROB_index;
  struct ROB_entry* rs;
  int num_searched = 0;

  /* walk ROB, remove all instructions from the IQ */
  ROB_index = contexts[c_id].ROB_head;
  if(contexts[c_id].ROB_num == 0){
    return 0;
  }
  
  /* find the next instruction awaiting dispatch */
  while ((rs = &contexts[c_id].ROB[ROB_index])){
    if(ROB_index == contexts[c_id].ROB_tail && num_searched){
      break;
    }
    if(rs->dispatched == FALSE){
      count++;
    }

    ROB_index = (ROB_index + 1) % ROB_size;
    num_searched++;
  }
  return count;
}


/* rename instructions from the IFETCH -> RENAME queue: instructions are
   first decoded, then they allocated ROB (and LSQ for load/stores) resources
   and then their registers are renamed */
static void
register_rename(void)
{
  int i;
  int n_renamed;		        /* total insts renameded */
  md_inst_t inst;			/* actual instruction bits */
  enum md_opcode op;			/* decoded opcode enum */
  int out1, out2, in1, in2, in3;	/* output/input register names */
  md_addr_t target_PC;			/* actual next/target PC address */
  md_addr_t addr;			/* effective address, if load/store */
  struct ROB_entry *rs;		        /* ROB entry being allocated */
  struct ROB_entry *lsq;		/* LSQ entry for ld/st's */
  struct bpred_update_t *dir_update_ptr;/* branch predictor dir update ptr */
  int stack_recover_idx;		/* bpred retstack recovery index */
  unsigned int pseq;			/* pipetrace sequence number */
  int is_write;				/* store? */
  int made_check;			/* used to ensure DLite entry */
  int br_taken, br_pred_taken;		/* if br, taken?  predicted taken? */
  int fetch_redirected[MAX_CONTEXTS];   /* indicates if fetch has been redirected for each thread
					*  due to a branch for example */
  int fetch_stalled[MAX_CONTEXTS];      /* indicates if fetch is stalled for each thread, due to
					 * a branch, or Icache miss */
  byte_t temp_byte = 0;			/* temp variable for spec mem access */
  half_t temp_half = 0;			/* " ditto " */
  word_t temp_word = 0;			/* " ditto " */
#ifdef HOST_HAS_QWORD
  qword_t temp_qword = 0;		/* " ditto " */
#endif /* HOST_HAS_QWORD */
  enum md_fault_type fault;             /* indicates if a fault has occured while decoding an instruction */
  int bitmap_context_id = 0;            /* used for identifying the context in bitmap MACROS */
  struct mem_t* mem;                    /* the memory space for the thread currently being examined */
  struct regs_t* regs;                  /* the arch register set for the thread currently being examined */
  struct regs_t* spec_regs;             /* the spec reg set for the thread currently being examined */
  static int disp_context_id = 0;       /* the ID of the context currently being examined */
  int spec_mode = contexts[disp_context_id].spec_mode; /* indicates if the current context is in speculative mode */

  /* Wattch:  Added for pop count generation (AFs) */
  quad_t val_ra, val_rb, val_rc, val_ra_result;

  made_check = FALSE;
  n_renamed = 0;

  /* initalize variables */
  for(i=0;i<MAX_CONTEXTS;i++){
    fetch_redirected[i]=FALSE;
    fetch_stalled[i] = FALSE;
  }

  /* round robbin dispatch */
  disp_context_id = (disp_context_id + 1) % num_contexts;
  spec_mode = contexts[disp_context_id].spec_mode;

  while (/* instruction decode B/W left? */
	 n_renamed < (decode_width)
	 /* on an acceptable trace path */
	 && (include_spec || !contexts[disp_context_id].spec_mode))
    {

      /* see if it is possible to rename from this thread */
      if((contexts[disp_context_id].fetch_num == 0)
	 /* check if the ROB is full */
	 || (contexts[disp_context_id].ROB_num >= ROB_size)
	 /* check if the LSQ is full */
	 || (contexts[disp_context_id].LSQ_num >= LSQ_size)
	 /* enforce the fetch to rename delay */
	 || (contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].fetched_cycle + FETCH_RENAME_DELAY > sim_cycle)){
	/* if it is not possible to rename instructions from this thread, then try to find another one */
	int j;
	int found = 0;
	for(j = 0; j<num_contexts; j++){
	  if((contexts[j].fetch_num != 0)
	     && (contexts[j].ROB_num < ROB_size)
	     && (contexts[j].LSQ_num < LSQ_size)
	     && (contexts[j].fetch_data[contexts[j].fetch_head].fetched_cycle + FETCH_RENAME_DELAY <= sim_cycle)
	     && (!fetch_stalled[j])){
	    disp_context_id = j;
	    found = 1;
	    break;
	  }
	}
	if(found){
	  continue;
	}
	else
	  break;
      }

      /* only permit decode_width*RENAME_DISPATCH_DELAY instructions in the rename->dispatch pipeline */
      if(not_dispatched_count(disp_context_id) >= (RENAME_DISPATCH_DELAY ? decode_width*RENAME_DISPATCH_DELAY : decode_width)){
	fetch_stalled[disp_context_id] = TRUE;
      }

      /* if fetch from the current thread is stalled, try to find a thread
       * that isn't stalled. If none exists, then stop renaming
       */
      if(fetch_stalled[disp_context_id]){
	int j;
	int all_stalled = 1;
	for(j = 0; j<num_contexts; j++){
	  if(!fetch_stalled[j])
	    all_stalled = 0;
	}
	if(all_stalled == 1)
	  break;
	else{
	  disp_context_id = (disp_context_id + 1) % num_contexts;
	  continue;
	}
      }


      /* TODO: Actually remove instructions from this queue during rob_recover() */
      if(fetch_redirected[disp_context_id]){
	/* consume instruction from IFETCH -> RENAME queue */
	contexts[disp_context_id].icount--;
	assert(contexts[disp_context_id].icount >= 0);
	contexts[disp_context_id].fetch_head = (contexts[disp_context_id].fetch_head+1) & (ifq_size - 1);
	contexts[disp_context_id].fetch_num--;
	continue;
      }

      /* if issuing in-order, block until last op issues if inorder issue */
      if (inorder_issue
	  && (last_op.rs && RSLINK_VALID(&last_op)
	      && !all_operands_ready(last_op.rs)))
	{
	  /* stall until last operation is ready to issue */
	  break;
	}


      /* get the next instruction from the IFETCH -> RENAME queue */
      bitmap_context_id = disp_context_id;
      mem = contexts[disp_context_id].mem;
      regs = &contexts[disp_context_id].regs;
      spec_regs = &contexts[disp_context_id].spec_regs;
      inst = contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].IR;
      regs->regs_PC = contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].regs_PC;
      contexts[disp_context_id].pred_PC = contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].pred_PC;
      dir_update_ptr = &(contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].dir_update);
      stack_recover_idx = contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].stack_recover_idx;
      pseq = contexts[disp_context_id].fetch_data[contexts[disp_context_id].fetch_head].ptrace_seq;
      spec_mode = contexts[disp_context_id].spec_mode;

      /* decode the inst */
      MD_SET_OPCODE(op, inst);

      {
	struct reg_set my_regs;
	get_reg_set(&my_regs, op);
	/* make sure we have a free physical register for the destination */
	if(my_regs.dest != REG_NONE){
	  if(find_free_physreg(my_regs.dest) < 0){
	    /* stall because there are no physical registers free */
	    fetch_stalled[disp_context_id] = TRUE;
	    continue;
	  }
	}
      }

      /* compute default next PC */
      regs->regs_NPC = regs->regs_PC + sizeof(md_inst_t);

      /* drain ROB for TRAPs and system calls */
      if (MD_OP_FLAGS(op) & F_TRAP)
	{
	  /* else, syscall is only instruction in the machine, at this
	     point we should not be in (mis-)speculative mode */
	  if (contexts[disp_context_id].spec_mode){
	    /* then stall: it will be flushed anyway */
	    fetch_stalled[disp_context_id] = TRUE;
	    continue;
	  }
	}

      /* maintain $r0 semantics (in spec and non-spec space) */
      regs->regs_R[MD_REG_ZERO] = 0; spec_regs->regs_R[MD_REG_ZERO] = 0;
      regs->regs_F.d[MD_REG_ZERO] = 0.0; spec_regs->regs_F.d[MD_REG_ZERO] = 0.0;

      if (!contexts[disp_context_id].spec_mode)
	{
	  /* one more non-speculative instruction executed */
	  sim_num_insn++;
	}

      /* default effective address (none) and access */
      addr = 0; is_write = FALSE;

      /* Wattch: Get values of source operands */
      val_ra = GPR(RA);
      val_rb = GPR(RB);

      /* set default fault - none */
      fault = md_fault_none;

      /* more decoding and execution */
      switch (op)
	{
#define DEFINST(OP,MSK,NAME,OPFORM,RES,CLASS,O1,O2,I1,I2,I3)		\
	case OP:							\
	  /* compute output/input dependencies to out1-2 and in1-3 */	\
	  out1 = O1; out2 = O2;						\
	  in1 = I1; in2 = I2; in3 = I3;					\
	  /* execute the instruction */					\
	  SYMCAT(OP,_IMPL);						\
	  break;
#define DEFLINK(OP,MSK,NAME,MASK,SHIFT)					\
	case OP:							\
	  /* could speculatively decode a bogus inst, convert to NOP */	\
	  op = MD_NOP_OP;						\
	  /* compute output/input dependencies to out1-2 and in1-3 */	\
	  out1 = NA; out2 = NA;						\
	  in1 = NA; in2 = NA; in3 = NA;					\
	  /* no EXPR */							\
	  break;
#define CONNECT(OP)	/* nada... */
	  /* the following macro wraps the instruction fault declaration macro
	     with a test to see if the trace generator is in non-speculative
	     mode, if so the instruction fault is declared, otherwise, the
	     error is shunted because instruction faults need to be masked on
	     the mis-speculated instruction paths */
#define DECLARE_FAULT(FAULT)						\
	  {								\
	    if (!contexts[disp_context_id].spec_mode)						\
	      fault = (FAULT);						\
	    /* else, spec fault, ignore it, always terminate exec... */	\
	    break;							\
	  }
#include "machine.def"
	default:
	  /* can speculatively decode a bogus inst, convert to a NOP */
	  op = MD_NOP_OP;
	  /* compute output/input dependencies to out1-2 and in1-3 */	\
	  out1 = NA; out2 = NA;
	  in1 = NA; in2 = NA; in3 = NA;
	  /* no EXPR */
	}
      /* operation sets next PC */

      /* print retirement trace if in verbose mode */
      if (!contexts[disp_context_id].spec_mode && verbose)
        {
          myfprintf(stderr, "++ %10n [xor: 0x%08x] {%d} @ 0x%08p: ",
                    sim_num_insn, md_xor_regs(regs),
                    inst_seq+1, regs->regs_PC);
          md_print_insn(inst, regs->regs_PC, stderr);
          fprintf(stderr, "\n");
          /* fflush(stderr); */
        }

      if (fault != md_fault_none){
	  fatal("non-speculative fault (%d) detected @ %d:0x%08p spec_mode: %d cxt.spec_mode %d",
		fault, disp_context_id, regs->regs_PC, spec_mode, contexts[disp_context_id].spec_mode);
      }

      /* Wattch: Get result values */
#if defined(TARGET_PISA)
      val_rc = GPR(RD);
      val_ra_result = GPR(RS);
#elif defined(TARGET_ALPHA)
      val_rc = GPR(RC);
      val_ra_result = GPR(RA);
#endif
      
      /* update memory access stats */
      if (MD_OP_FLAGS(op) & F_MEM)
	{
	  sim_total_refs++;
	  if (!contexts[disp_context_id].spec_mode)
	    sim_num_refs++;

	  if (MD_OP_FLAGS(op) & F_STORE)
	    is_write = TRUE;
	  else
	    {
	      sim_total_loads++;
	      if (!contexts[disp_context_id].spec_mode)
		sim_num_loads++;
	    }
	}

      br_taken = (regs->regs_NPC != (regs->regs_PC + sizeof(md_inst_t)));
      br_pred_taken = (contexts[disp_context_id].pred_PC != (regs->regs_PC + sizeof(md_inst_t)));

      if ((contexts[disp_context_id].pred_PC != regs->regs_NPC && pred_perfect)
	  || ((MD_OP_FLAGS(op) & (F_CTRL|F_DIRJMP)) == (F_CTRL|F_DIRJMP)
	      && target_PC != contexts[disp_context_id].pred_PC && br_pred_taken))
	{
	  /* Either 1) we're simulating perfect prediction and are in a
             mis-predict state and need to patch up, or 2) We're not simulating
             perfect prediction, we've predicted the branch taken, but our
             predicted target doesn't match the computed target (i.e.,
             mis-fetch).  Just update the PC values and do a fetch squash.
             This is just like calling fetch_squash() except we pre-anticipate
             the updates to the fetch values at the end of this function.  If
             case #2, also charge a mispredict penalty for redirecting fetch */
	  contexts[disp_context_id].fetch_pred_PC = contexts[disp_context_id].fetch_regs_PC = 
	    contexts[disp_context_id].regs.regs_NPC;
	  if (pred_perfect)
	    contexts[disp_context_id].pred_PC = contexts[disp_context_id].regs.regs_NPC;

	  fetch_redirected[disp_context_id] = TRUE;
	}


      /* is this a NOP */
      if (op != MD_NOP_OP)
	{
	  /* for load/stores:
	         idep #0     - store operand (value that is store'ed)
	         idep #1, #2 - eff addr computation inputs (addr of access)

	     resulting ROB/LSQ operation pair:
	       ROB (effective address computation operation):
		 idep #0, #1 - eff addr computation inputs (addr of access)
	       LSQ (memory access operation):
		 idep #0     - operand input (value that is store'd)
		 idep #1     - eff addr computation result (from ROB op)

	     effective address computation is transfered via the reserved
	     name DTMP
	   */

	  /* Wattch -- Dispatch + RAT lookup stage */
	  rename_access++;

	  /* fill in ROB entry */
	  rs = &contexts[disp_context_id].ROB[contexts[disp_context_id].ROB_tail];
          rs->slip = sim_cycle - 1;
	  rs->IR = inst;
	  rs->op = op;
	  rs->PC = regs->regs_PC;
	  rs->next_PC = regs->regs_NPC; rs->pred_PC = contexts[disp_context_id].pred_PC;
	  rs->in_LSQ = FALSE;
	  rs->LSQ_index = -1;
	  rs->ea_comp = FALSE;
	  rs->recover_inst = FALSE;
          rs->dir_update = *dir_update_ptr;
	  rs->stack_recover_idx = stack_recover_idx;
	  rs->spec_mode = contexts[disp_context_id].spec_mode;
	  rs->addr = 0;
	  /* rs->tag is already set */
	  rs->replayed = FALSE;
	  rs->seq = ++inst_seq;
	  rs->in_IQ = rs->dispatched = rs->queued = rs->issued = rs->completed = FALSE;
	  rs->ptrace_seq = pseq;
	  rs->context_id = disp_context_id;
	  rs->rename_cycle = sim_cycle;
	  rs->disp_cycle = 0;
	  /* store the architectural registers in the ROB entry */
	  rs->archreg = out1;
	  rs->src_archreg[0] = in1;
	  rs->src_archreg[1] = in2;
	  /* store the physical source registers */
	  rs->src_physreg[0] = contexts[disp_context_id].rename_table[in1];
	  rs->src_physreg[1] = contexts[disp_context_id].rename_table[in2];

	  if((out1 != 0) && (out1 != 32))
	  {
	    /* Register renaming support */
	    struct reg_set my_regs;
	    get_reg_set(&my_regs, op);

	    /* allocate and store the physical register for the destination */
	    switch(my_regs.dest){
	    case REG_NONE:
	      rs->physreg = -1;
	      rs->old_physreg = -1;
	      break;
	    case REG_INT:
	    case REG_FP:
	      rs->physreg = alloc_physreg(rs);
	      assert(rs->physreg >= 0);
	      break;
	    }
	    rs->dest_format = my_regs.dest;

	  }else{
	    /* DON'T RENAME REGISTER 0 */
	    rs->physreg = -1;
	    rs->old_physreg = -1;
	    rs->dest_format = REG_NONE;
	  }

	  /* Wattch: Maintain values through core for AFs*/
	  rs->val_ra = val_ra;
	  rs->val_rb = val_rb;
	  rs->val_rc = val_rc;
	  rs->val_ra_result = val_ra_result;


	  /* split ld/st's into two operations: eff addr comp + mem access */
	  if (MD_OP_FLAGS(op) & F_MEM)
	    {
	      /* convert ROB operation from ld/st to an add (eff addr comp) */
	      rs->op = MD_AGEN_OP;
	      rs->ea_comp = TRUE;

	      /* fill in LSQ entry */
	      lsq = &contexts[disp_context_id].LSQ[contexts[disp_context_id].LSQ_tail];
              lsq->slip = sim_cycle - 1;
	      lsq->IR = inst;
	      lsq->op = op;
	      lsq->PC = regs->regs_PC;
	      lsq->next_PC = regs->regs_NPC; lsq->pred_PC = contexts[disp_context_id].pred_PC;
	      rs->LSQ_index = contexts[disp_context_id].LSQ_tail;
	      lsq->in_LSQ = TRUE;
	      lsq->in_IQ = FALSE;
	      lsq->ea_comp = FALSE;
	      lsq->dispatched = FALSE;
	      lsq->recover_inst = FALSE;
	      lsq->dir_update.pdir1 = lsq->dir_update.pdir2 = NULL;
	      lsq->dir_update.pmeta = NULL;
	      lsq->stack_recover_idx = 0;
	      lsq->spec_mode = contexts[disp_context_id].spec_mode;
	      lsq->addr = addr;
	      /* lsq->tag is already set */
	      lsq->seq = ++inst_seq;
	      lsq->replayed = FALSE;
	      lsq->queued = lsq->issued = lsq->completed = FALSE;
	      lsq->ptrace_seq = pseq;
	      lsq->context_id = disp_context_id;
	      lsq->disp_cycle = 0;
	      lsq->rename_cycle = sim_cycle;
	      lsq->archreg = out1;
	      lsq->src_archreg[0] = in1;
	      lsq->src_archreg[1] = in2;
	      lsq->src_physreg[0] = rs->src_physreg[0];
	      lsq->src_physreg[1] = rs->src_physreg[1];
	      lsq->physreg = rs->physreg;
	      lsq->old_physreg = -1;
	      lsq->dest_format = rs->dest_format;

	      /* Wattch: Maintain values through core for AFs*/
	      lsq->val_ra = val_ra;
	      lsq->val_rb = val_rb;
	      lsq->val_rc = val_rc;
	      lsq->val_ra_result = val_ra_result;

	      /* pipetrace this uop */
	      ptrace_newuop(lsq->ptrace_seq, "internal ld/st", lsq->PC, 0);
	      ptrace_newstage(lsq->ptrace_seq, PST_DISPATCH, 0);

	      /* install operation in the ROB and LSQ */
	      n_renamed++;
	      contexts[disp_context_id].ROB_tail = (contexts[disp_context_id].ROB_tail + 1) % ROB_size;
	      contexts[disp_context_id].ROB_num++;
	      contexts[disp_context_id].LSQ_tail = (contexts[disp_context_id].LSQ_tail + 1) % LSQ_size;
	      contexts[disp_context_id].LSQ_num++;

	      /* issue may continue when the load/store is issued */
	      RSLINK_INIT(last_op, lsq);

	    }
	  else /* !(MD_OP_FLAGS(op) & F_MEM) */
	    {
	      /* Wattch: Regfile writes taken care of inside ruu_link_idep */

	      /* install operation in the ROB */
	      n_renamed++;
	      contexts[disp_context_id].ROB_tail = (contexts[disp_context_id].ROB_tail + 1) % ROB_size;
	      contexts[disp_context_id].ROB_num++;

	    }
	}
      else
	{
	  /* this is a NOP, no need to update ROB/LSQ state */
	  contexts[disp_context_id].icount--;
	  rs = NULL;
	}
      
      /* one more instruction executed, speculative or otherwise */
      sim_total_insn++;
      if (MD_OP_FLAGS(op) & F_CTRL)
	sim_total_branches++;
      
      if (!contexts[disp_context_id].spec_mode)
	{
	  /* if this is a branching instruction update BTB, i.e., only
	     non-speculative state is committed into the BTB */
	  if (MD_OP_FLAGS(op) & F_CTRL)
	    {
	      sim_num_branches++;
	      if (contexts[disp_context_id].pred && bpred_spec_update == spec_ID)
		{
		  bpred_update(contexts[disp_context_id].pred,
			       /* branch address */regs->regs_PC,
			       /* actual target address */regs->regs_NPC,
			       /* taken? */regs->regs_NPC != (regs->regs_PC +
						       sizeof(md_inst_t)),
			       /* pred taken? */contexts[disp_context_id].pred_PC != (regs->regs_PC +
							sizeof(md_inst_t)),
			       /* correct pred? */contexts[disp_context_id].pred_PC == regs->regs_NPC,
			       /* opcode */op,
			       /* predictor update ptr */&rs->dir_update);
		}
	    }


	  assert(regs->regs_NPC == rs->next_PC);

	  /* is the trace generator trasitioning into mis-speculation mode? */
	  if (contexts[disp_context_id].pred_PC != regs->regs_NPC)
	    {
	      /* entering mis-speculation mode, indicate this and save PC */
	      contexts[disp_context_id].spec_mode = TRUE;
	      rs->recover_inst = TRUE;
	      contexts[disp_context_id].recover_PC = regs->regs_NPC;
	    }
	}

      /* entered decode/allocate stage, indicate in pipe trace */
      ptrace_newstage(pseq, PST_DISPATCH,
		      (contexts[disp_context_id].pred_PC != regs->regs_NPC) ? PEV_MPOCCURED : 0);
      if (op == MD_NOP_OP)
	{
	  /* end of the line */
	  ptrace_endinst(pseq);
	}

      /* update any stats tracked by PC */
      for (i=0; i<pcstat_nelt; i++)
	{
	  counter_t newval;
	  int delta;

	  /* check if any tracked stats changed */
	  newval = STATVAL(pcstat_stats[i]);
	  delta = newval - pcstat_lastvals[i];
	  if (delta != 0)
	    {
	      stat_add_samples(pcstat_sdists[i], regs->regs_PC, delta);
	      pcstat_lastvals[i] = newval;
	    }
	}

      /* consume instruction from IFETCH -> RENAME queue */
      contexts[disp_context_id].fetch_head = (contexts[disp_context_id].fetch_head+1) & (ifq_size - 1);
      contexts[disp_context_id].fetch_num--;

      /* check for DLite debugger entry condition */
      made_check = TRUE;
      if (dlite_check_break(contexts[disp_context_id].pred_PC,
			    is_write ? ACCESS_WRITE : ACCESS_READ,
			    addr, sim_num_insn, sim_cycle))
	dlite_main(regs->regs_PC, contexts[disp_context_id].pred_PC, sim_cycle, regs, mem);

    }

  /* need to enter DLite at least once per cycle */
  if (!made_check)
    {
      if (dlite_check_break(/* no next PC */0,
			    is_write ? ACCESS_WRITE : ACCESS_READ,
			    addr, sim_num_insn, sim_cycle))
	dlite_main(regs->regs_PC, /* no next PC */0, sim_cycle, regs, contexts[disp_context_id].mem);
    }

}




/*
 * dispatches instruction into the IQ
 */
static void
dispatch(void)
{
  int i;
  int n_dispatched;		        /* total insts renameded */
  struct ROB_entry *rs;		        /* ROB entry being allocated */
  struct ROB_entry *lsq;		/* LSQ entry for ld/st's */
  int disp_stalled[MAX_CONTEXTS];
  int num_dispatched[MAX_CONTEXTS];
  static int disp_context_id = 0;       /* the ID of the context currently being examined */
  int my_iq_num = -1;                   /* the IQ entry allocated for this intruction */
  int ROB_index[MAX_CONTEXTS];
  int num_searched = 0;

  n_dispatched = 0;
  
  /* initalize variables */
  for(i=0;i<MAX_CONTEXTS;i++){
    ROB_index[i] = contexts[i].ROB_head;
    disp_stalled[i] = FALSE;
    num_dispatched[i] = 0;
  }
  
  /* round robbin dispatch */
  disp_context_id = (disp_context_id + 1) % num_contexts;
  
  while(n_dispatched < decode_width){
    /* see if all threads are stalled */
    {
      int j;
      int all_stalled = 1;
      
      for(j = 0; j<num_contexts; j++){
	if(!disp_stalled[j])
	  all_stalled = 0;
      }

      if(all_stalled)
	break;
    }

    /* get the next instruction awaiting dispatch */
    rs = (contexts[disp_context_id].ROB_num ? &contexts[disp_context_id].ROB[ROB_index[disp_context_id]] : NULL);

    if(!rs){
      /* if there are no more instructions waiting dispatch for this thread, try another thread */
      disp_stalled[disp_context_id] = 1;
      disp_context_id = (disp_context_id + 1) % num_contexts;
      continue;
    }

    /* find the next instruction awaiting dispatch */
    while (!disp_stalled[disp_context_id]){
      if(ROB_index[disp_context_id] == contexts[disp_context_id].ROB_head && num_searched == 0){
	if(rs->dispatched == 0){
	  break;
	}
      }else if(ROB_index[disp_context_id] == contexts[disp_context_id].ROB_tail){
	ROB_index[disp_context_id] = -1;
	disp_stalled[disp_context_id] = 1;
	break;
      }
      num_searched++;

      if(rs->dispatched == 0){
	break;
      }
      ROB_index[disp_context_id] = (ROB_index[disp_context_id] + 1) % ROB_size;
      rs = &contexts[disp_context_id].ROB[ROB_index[disp_context_id]];
    }

    /* if we can't dispatch from this thread, try another thread */
    if(disp_stalled[disp_context_id]){
      disp_context_id = (disp_context_id + 1) % num_contexts;
      continue;
    }

    /* enforce the rename-to-dispatch delay */
    if(rs->rename_cycle + RENAME_DISPATCH_DELAY > sim_cycle){
      disp_stalled[disp_context_id] = 1;
      disp_context_id = (disp_context_id + 1) % num_contexts;
      continue;
    }


    /* we now have a vaild instruction in RS to dispatch */
    num_dispatched[disp_context_id]++;

    /* if there are no available IQ slots, then block this thread */
    my_iq_num = alloc_iq_entry();
    if(my_iq_num < 0){
	disp_stalled[disp_context_id] = TRUE;
	continue;
    }

    /* update the ROB entry */
    rs->disp_cycle = sim_cycle;
    rs->dispatched = TRUE;
    
    /* insert into the issue queue */
    rs->iq_entry_num = my_iq_num;
    rs->in_IQ = TRUE;
    assert(iq[my_iq_num] != IQ_ENTRY_FREE);
    
    if (all_operands_spec_ready(rs))
      {
	/* Wattch -- both operands ready, 2 window write accesses */
	/* Wattch -- FIXME: currently being read from arch.
	   regfile (in ruu_link_idep) and written to window here.
	   should these values be read from arch. regfile or 
	   another window entry? */
	window_access++;
	window_access++;
	window_preg_access++;
	window_preg_access++;
	
#ifdef DYNAMIC_AF	
	regfile_total_pop_count_cycle += pop_count(rs->val_ra);
	regfile_total_pop_count_cycle += pop_count(rs->val_rb);
	regfile_num_pop_count_cycle+=2;
#endif
	
	/* eff addr computation ready, queue it on ready list */
	assert(!rs->queued);
	readyq_enqueue(rs);
      }
    else if (one_operand_ready(rs))
      {
	/* Wattch -- one operand ready, 1 window write accesses */
	window_access++;
	window_preg_access++;
#ifdef DYNAMIC_AF	
	if(operand_ready(rs,0))
	  regfile_total_pop_count_cycle += pop_count(rs->val_ra);
	else
	  regfile_total_pop_count_cycle += pop_count(rs->val_rb);
	regfile_num_pop_count_cycle++;
#endif
      }

    if(rs->LSQ_index != -1){
      lsq = &contexts[disp_context_id].LSQ[rs->LSQ_index];
      lsq->disp_cycle = sim_cycle;
      lsq->dispatched = TRUE;

      /* issue stores only, loads are issued by lsq_refresh() */
      if(MD_OP_FLAGS(lsq->op) & F_STORE){
	if(all_operands_spec_ready(lsq)){
	  /* Wattch -- store operand ready, 1 LSQ access */
	  lsq_store_data_access++;
	  
	  /* panic("store immediately ready"); */
	  /* put operation on ready list, selection() issue it later */
	  assert(!lsq->queued);
	  readyq_enqueue(lsq);
	}else{
	  assert(!lsq->queued);
	  wait_q_enqueue(lsq, sim_cycle);
	}
      }
    }
    
    if(!rs->queued)
      wait_q_enqueue(rs, sim_cycle);

    n_dispatched++;
  }
}

/*
 *  FETCH() - instruction fetch pipeline stage(s)
 */

/* initialize the instruction fetch pipeline stage */
static void
fetch_init(void)
{
  int c = 0;
  for(c=0;c<MAX_CONTEXTS;c++){
    /* allocate the IFETCH -> RENAME instruction queue */
    contexts[c].fetch_data =
      (struct fetch_rec *)calloc(ifq_size, sizeof(struct fetch_rec));
    if (! contexts[c].fetch_data)
      fatal("out of virtual memory");
    
    contexts[c].fetch_num = 0;
    contexts[c].fetch_tail =  contexts[c].fetch_head = 0;
  }
}

/* dump contents of fetch stage registers and fetch queue */
void
fetch_dump(FILE *stream, int context_id)       	/* output stream */
{
  int num, head;

  if (!stream)
    stream = stderr;

  fprintf(stream, "** fetch stage state **\n");

  fprintf(stream, "spec_mode: %s\n", contexts[context_id].spec_mode ? "t" : "f");
  myfprintf(stream, "pred_PC: 0x%08p, recover_PC: 0x%08p\n",
	    contexts[context_id].pred_PC, contexts[context_id].recover_PC);
  myfprintf(stream, "fetch_regs_PC: 0x%08p, fetch_pred_PC: 0x%08p\n",
	    contexts[context_id].fetch_regs_PC, contexts[context_id].fetch_pred_PC);
  fprintf(stream, "\n");

  fprintf(stream, "** fetch queue contents **\n");
  fprintf(stream, "fetch_num: %d\n", contexts[context_id].fetch_num);
  fprintf(stream, "fetch_head: %d, fetch_tail: %d\n",
	  contexts[context_id].fetch_head, contexts[context_id].fetch_tail);

  num = contexts[context_id].fetch_num;
  head = contexts[context_id].fetch_head;
  while (num)
    {
      fprintf(stream, "idx: %2d: inst: `", head);
      md_print_insn(contexts[context_id].fetch_data[head].IR, contexts[context_id].fetch_data[head].regs_PC, stream);
      fprintf(stream, "'\n");
      myfprintf(stream, "         regs_PC: 0x%08p, pred_PC: 0x%08p\n",
		contexts[context_id].fetch_data[head].regs_PC, contexts[context_id].fetch_data[head].pred_PC);
      head = (head + 1) & (ifq_size - 1);
      num--;
    }
}

static int last_inst_missed = FALSE;
static int last_inst_tmissed = FALSE;


/* fetch up as many instruction as one branch prediction and one cache line
   acess will support without overflowing the IFETCH -> RENAME QUEUE */
static void
fetch(int* current_fetch_context_ptr)
{
  int i, lat, tlb_lat;
  int done[MAX_CONTEXTS];
  md_inst_t inst;
  int stack_recover_idx;
  int branch_cnt[MAX_CONTEXTS];
  int current_fetch_context = current_fetch_context_ptr[0];

  for(i=0;i<MAX_CONTEXTS;i++){
    branch_cnt[i] = 0;
    done[i] = FALSE;
  }
  
  if(contexts[0].fetch_num == ifq_size){
    current_fetch_context_ptr[0] =  current_fetch_context_ptr[1];
    current_fetch_context_ptr[1] =  current_fetch_context_ptr[2];
    current_fetch_context_ptr[2] =  current_fetch_context_ptr[3];
  }
  if(contexts[0].fetch_num == ifq_size){
    current_fetch_context_ptr[0] =  current_fetch_context_ptr[1];
    current_fetch_context_ptr[1] =  current_fetch_context_ptr[2];
    current_fetch_context_ptr[2] =  current_fetch_context_ptr[3];
  }
  

  for (i=0;
       /* fetch up to as many instruction as the RENAME stage can decode */
       i < (decode_width * fetch_speed);
       /* and no IFETCH blocking condition encountered */
       i++)
    {

      
      /* fetch until IFETCH -> RENAME queue fills */
      if(contexts[current_fetch_context].fetch_num == ifq_size){
	if(current_fetch_context != current_fetch_context_ptr[1]){
	  current_fetch_context = current_fetch_context_ptr[1];
	}
	else
	  return;
	i--;
	continue;
      }

      /* stop fetching if we reach a branch */
      if (branch_cnt[current_fetch_context] > 0){
	if(current_fetch_context != current_fetch_context_ptr[1]){
	  current_fetch_context = current_fetch_context_ptr[1];
	  if (branch_cnt[current_fetch_context]){
	    return;
	  }
	}
	else
	  return;
	i--;
	continue;
      }

      if (done[current_fetch_context] > 0){
	if(current_fetch_context != current_fetch_context_ptr[1]){
	  current_fetch_context = current_fetch_context_ptr[1];
	  if (done[current_fetch_context])
	    return;
	}
	else{
	  return;
	}
	i--;
	continue;
      }
      
      /* enforce the fetch-issue delay counters for each thread
       * these counters are used for modeling the minimum branch misprediction
       * penalty.
       */
      if (contexts[current_fetch_context].fetch_issue_delay > 0){
	if(current_fetch_context != current_fetch_context_ptr[1]){
	  current_fetch_context = current_fetch_context_ptr[1];
	  if (contexts[current_fetch_context].fetch_issue_delay > 0){
	    return;
	  }
	}
	else{
	  return;
	}
	i--;
	continue;
	}
      
      struct mem_t* mem = contexts[current_fetch_context].mem;
      /* fetch an instruction at the next predicted fetch address */
      contexts[current_fetch_context].fetch_regs_PC = contexts[current_fetch_context].fetch_pred_PC;

      assert(current_fetch_context < num_contexts);

     /* Wattch: add power for i-fetch stage */
      icache_access++;

      /* is this a bogus text address? (can happen on mis-spec path) */
      if (mem->ld_text_base <= contexts[current_fetch_context].fetch_regs_PC
	  && contexts[current_fetch_context].fetch_regs_PC < (mem->ld_text_base+mem->ld_text_size)
	  && !(contexts[current_fetch_context].fetch_regs_PC & (sizeof(md_inst_t)-1)))
	{
	  /* read instruction from memory */
	  MD_FETCH_INST(inst, mem, contexts[current_fetch_context].fetch_regs_PC);

	  /* address is within program text, read instruction from memory */
	  lat = cache_il1_lat;
	  if (cache_il1)
	    {
	      /* access the I-cache */
	      lat =
		cache_access(cache_il1, Read, IACOMPRESS(contexts[current_fetch_context].fetch_regs_PC),
			     current_fetch_context, NULL, ISCOMPRESS(sizeof(md_inst_t)), sim_cycle,
			     NULL, NULL);
	      if (lat > cache_il1_lat)
		last_inst_missed = TRUE;
	    }

	  if (itlb)
	    {
	      /* access the I-TLB, NOTE: this code will initiate
		 speculative TLB misses */
	      tlb_lat =
		cache_access(itlb, Read, IACOMPRESS(contexts[current_fetch_context].fetch_regs_PC),
			     current_fetch_context, NULL, ISCOMPRESS(sizeof(md_inst_t)), sim_cycle,
			     NULL, NULL);
	      if (tlb_lat > 1)
		last_inst_tmissed = TRUE;

	      /* I-cache/I-TLB accesses occur in parallel */
	      lat = MAX(tlb_lat, lat);
	    }
	  
	  /* I-cache/I-TLB miss? assumes I-cache hit >= I-TLB hit */
	  if (lat != cache_il1_lat)
	    {
	      /* I-cache miss, block fetch until it is resolved */
	      contexts[current_fetch_context].fetch_issue_delay += lat - 1;
	    }
	  /* else, I-cache/I-TLB hit */
	}
      else
	{
	  /* fetch PC is bogus, send a NOP down the pipeline */
	  inst = MD_NOP_INST;
	}

      /* have a valid inst, here */

      /* possibly use the BTB target */
      if (contexts[current_fetch_context].pred)
	{
	  enum md_opcode op;

	  /* pre-decode instruction, used for bpred stats recording */
	  MD_SET_OPCODE(op, inst);
	  
	  /* get the next predicted fetch address; only use branch predictor
	     result for branches (assumes pre-decode bits); NOTE: returned
	     value may be 1 if bpred can only predict a direction */
	  if (MD_OP_FLAGS(op) & F_CTRL)
	    contexts[current_fetch_context].fetch_pred_PC =
	      bpred_lookup(contexts[current_fetch_context].pred,
			   /* branch address */contexts[current_fetch_context].fetch_regs_PC,
			   /* target address */0,
			   /* opcode */op,
			   /* call? */MD_IS_CALL(op),
			   /* return? */MD_IS_RETURN(op),
			   /* updt */&(contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].dir_update),
			   /* RSB index */&stack_recover_idx);
	  else
	    contexts[current_fetch_context].fetch_pred_PC = 0;

	  /* valid address returned from branch predictor? */
	  if (!contexts[current_fetch_context].fetch_pred_PC)
	    {
	      /* no predicted taken target, attempt not taken target */
	      contexts[current_fetch_context].fetch_pred_PC = contexts[current_fetch_context].fetch_regs_PC + sizeof(md_inst_t);
	    }
	  else
	    {
	      /* go with target, NOTE: discontinuous fetch, so terminate */
	      branch_cnt[current_fetch_context]++;
	      done[current_fetch_context] = TRUE;
	    }
	}
      else
	{
	  /* no predictor, just default to predict not taken, and
	     continue fetching instructions linearly */
	  contexts[current_fetch_context].fetch_pred_PC = contexts[current_fetch_context].fetch_regs_PC + sizeof(md_inst_t);
	}

      /* commit this instruction to the IFETCH -> RENAME queue */
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].IR = inst;
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].regs_PC = contexts[current_fetch_context].fetch_regs_PC;
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].pred_PC = contexts[current_fetch_context].fetch_pred_PC;
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].stack_recover_idx = stack_recover_idx;
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].ptrace_seq = contexts[current_fetch_context].ptrace_seq++;
      contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].fetched_cycle = sim_cycle;

      /* for pipe trace */
      ptrace_newinst(contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].ptrace_seq,
		     inst, contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].regs_PC,
		     0);
      ptrace_newstage(contexts[current_fetch_context].fetch_data[contexts[current_fetch_context].fetch_tail].ptrace_seq,
		      PST_IFETCH,
		      ((last_inst_missed ? PEV_CACHEMISS : 0)
		       | (last_inst_tmissed ? PEV_TLBMISS : 0)));
      last_inst_missed = FALSE;
      last_inst_tmissed = FALSE;

      /* adjust instruction fetch queue */
      contexts[current_fetch_context].fetch_tail = (contexts[current_fetch_context].fetch_tail + 1) & (ifq_size - 1);
      contexts[current_fetch_context].fetch_num++;
      contexts[current_fetch_context].icount++;

      /* fetch half from first thread, half from second... */
      if(i == (decode_width * fetch_speed)/2)
	current_fetch_context = current_fetch_context_ptr[1]; 
    }
}

/* used for icount fetch */
int my_comparator(const void * a1, const void * a2){
  return (contexts[*(int*)a1].icount - contexts[*(int*)a2].icount);
}


/* ICOUNT fetch policy */
static void icount_fetch(void){
  int i;
  int sorted_contexts[MAX_CONTEXTS];

  for(i=0;i<num_contexts;i++){
    sorted_contexts[i]= i;
    assert(contexts[i].icount >= 0);
    assert(contexts[i].fetch_num <= ifq_size);
    assert(contexts[i].icount <= 2*(iq_size + ifq_size));
  }

  for(i=num_contexts; i<MAX_CONTEXTS; i++)
    sorted_contexts[i] = num_contexts - 1;

  qsort(&sorted_contexts, num_contexts, sizeof(int), &my_comparator);

  for(i=0;i<MAX_CONTEXTS;i++){
    assert(sorted_contexts[i] < num_contexts);
  }

  /* fetch instructions */
  fetch(sorted_contexts);

}

/* round robbin fetch policy */
static void RR_fetch(int context){
  
  int i;
  int sorted_contexts[MAX_CONTEXTS];

  for(i=0;i<MAX_CONTEXTS;i++){
    sorted_contexts[i]= (context + i) % num_contexts;
  }
  for(i=0;i<MAX_CONTEXTS;i++){
    assert(sorted_contexts[i] < num_contexts);
  }

  /* fetch instructions */
  fetch(sorted_contexts);

}


/* default machine state accessor, used by DLite */
static char *					/* err str, NULL for no err */
simoo_mstate_obj(FILE *stream,			/* output stream */
		 char *cmd,			/* optional command string */
		 struct regs_t *regs,		/* registers to access */
		 struct mem_t *mem)		/* memory space to access */
{
  if (!cmd || !strcmp(cmd, "help"))
    fprintf(stream,
"mstate commands:\n"
"\n"
"    mstate help   - show all machine-specific commands (this list)\n"
"    mstate stats  - dump all statistical variables\n"
"    mstate res    - dump current functional unit resource states\n"
"    mstate ruu    - dump contents of the register update unit\n"
"    mstate lsq    - dump contents of the load/store queue\n"
"    mstate eventq - dump contents of event queue\n"
"    mstate readyq - dump contents of ready instruction queue\n"
"    mstate rspec  - dump contents of speculative regs\n"
"    mstate mspec  - dump contents of speculative memory\n"
"    mstate fetch  - dump contents of fetch stage registers and fetch queue\n"
"\n"
	    );
  else if (!strcmp(cmd, "stats"))
    {
      /* just dump intermediate stats */
      sim_print_stats(stream);
    }
  else if (!strcmp(cmd, "res"))
    {
      /* dump resource state */
      res_dump(fu_pool, stream);
    }
  else if (!strcmp(cmd, "ruu"))
    {
      /* dump ROB contents */
      rob_dump(stream,mem->context_id);
    }
  else if (!strcmp(cmd, "lsq"))
    {
      /* dump LSQ contents */
      lsq_dump(stream, regs->context_id);
    }
  else if (!strcmp(cmd, "eventq"))
    {
      /* dump event queue contents */
      eventq_dump(stream, regs->context_id);
    }
  else if (!strcmp(cmd, "readyq"))
    {
      /* dump event queue contents */
      readyq_dump(stream, regs->context_id);
    }
  else if (!strcmp(cmd, "rspec"))
    {
      /* dump event queue contents */
      rspec_dump(stream);
    }
  else if (!strcmp(cmd, "mspec"))
    {
      /* dump event queue contents */
      mspec_dump(stream, regs->context_id);
    }
  else if (!strcmp(cmd, "fetch"))
    {
      /* dump event queue contents */
      fetch_dump(stream, regs->context_id);
    }
  else
    return "unknown mstate command";

  /* no error */
  return NULL;
}

/* returns the context_id of the context with the largest
   number of instructions left to fast forward  */
int
max_ff_left(){
  int max_id = 0;
  int i;

  for(i=1;i<num_contexts;i++){
    if(contexts[i].fastfwd_left > contexts[max_id].fastfwd_left)
      max_id = i;
  }

  if(contexts[max_id].fastfwd_left == 0)
    return -1;
  
  return max_id;
}


/* start simulation, program loaded, processor precise state initialized */
void
sim_main(void)
{
  int i;
  /* ignore any floating point exceptions, they may occur on mis-speculated
     execution paths */
  signal(SIGFPE, SIG_IGN);

  /* load the first context... */
  int current_context = num_contexts - 1;

  /* check for DLite debugger entry condition */
  if (dlite_check_break(contexts[0].regs.regs_PC, /* no access */0, /* addr */0, 0, 0)){
    dlite_main(contexts[0].regs.regs_PC, contexts[0].regs.regs_PC + sizeof(md_inst_t),
	       sim_cycle, &contexts[0].regs, contexts[0].mem);
  }

  if (rf_size < (32 * num_contexts + 1))
    fatal("Must have enough registers for arch state (32 for each thread)!!");



  /* if fastfwd_count is 1, then use the fastfwd numbers in the .arg files
   * otherwise use the specified fastfwd_count from each thread
   */
  if (fastfwd_count > 1){
    int i;
    for(i=0;i<num_contexts;i++)
      contexts[i].fastfwd_left = fastfwd_count;
  }

  /* fast forward simulator loop, performs functional simulation for
     FASTFWD_COUNT insts, then turns on performance (timing) simulation */
  if (fastfwd_count > 0)
    {
      md_inst_t inst;			/* actual instruction bits */
      enum md_opcode op;		/* decoded opcode enum */
      md_addr_t target_PC;		/* actual next/target PC address */
      md_addr_t addr;			/* effective address, if load/store */
      int is_write;			/* store? */
      byte_t temp_byte = 0;		/* temp variable for spec mem access */
      half_t temp_half = 0;		/* " ditto " */
      word_t temp_word = 0;		/* " ditto " */
#ifdef HOST_HAS_QWORD
      qword_t temp_qword = 0;		/* " ditto " */
#endif /* HOST_HAS_QWORD */
      enum md_fault_type fault;
      struct bpred_update_t dir_update[MAX_CONTEXTS];
      int stack_recover_idx[MAX_CONTEXTS];

      fprintf(stderr, "sim: ** fast forwarding insts **\n");

      /* while some context has FF instructions left */
      while((current_context = max_ff_left()) >= 0)
	{
	  struct mem_t* mem = contexts[current_context].mem;
	  struct regs_t* regs = &contexts[current_context].regs;
	  struct regs_t* spec_regs = &contexts[current_context].spec_regs;
	  int spec_mode = contexts[current_context].spec_mode;
	  int bitmap_context_id = current_context;
	  
	  /* access the I-cache */
	  cache_access(cache_il1, Read, IACOMPRESS(regs->regs_PC),
		       current_context, NULL, ISCOMPRESS(sizeof(md_inst_t)), sim_cycle,
		       NULL, NULL);
	  if (itlb)
	    {
	      /* access the I-TLB */
	      cache_access(itlb, Read, IACOMPRESS(regs->regs_PC),
			   current_context, NULL, ISCOMPRESS(sizeof(md_inst_t)), sim_cycle,
			   NULL, NULL);
	      
	    }

	  assert(current_context == mem->context_id);

	  /* maintain $r0 semantics */
	  regs->regs_R[MD_REG_ZERO] = 0;
	  regs->regs_F.d[MD_REG_ZERO] = 0.0;

	  /* get the next instruction to execute */
	  MD_FETCH_INST(inst, mem, regs->regs_PC);

	  /* set default reference address */
	  addr = 0; is_write = FALSE;

	  /* set default fault - none */
	  fault = md_fault_none;

	  /* decode the instruction */
	  MD_SET_OPCODE(op, inst);

	  /* execute the instruction */
	  switch (op)
	    {
#define DEFINST(OP,MSK,NAME,OPFORM,RES,FLAGS,O1,O2,I1,I2,I3)		\
	    case OP:							\
	      SYMCAT(OP,_IMPL);						\
	      break;
#define DEFLINK(OP,MSK,NAME,MASK,SHIFT)					\
	    case OP:							\
	      panic("attempted to execute a linking opcode");
#define CONNECT(OP)
#undef DECLARE_FAULT
#define DECLARE_FAULT(FAULT)						\
	      { fault = (FAULT); break; }
#include "machine.def"
	    default:
	      panic("attempted to execute a bogus opcode");
	    }

	  if (fault != md_fault_none)
	    fatal("fault (%d) detected @ 0x%08p", fault, regs->regs_PC);

	  /* update memory access stats */
	  if (MD_OP_FLAGS(op) & F_MEM)
	    {
	      if (MD_OP_FLAGS(op) & F_STORE)
		{
		  is_write = TRUE;
		  
		  cache_access(cache_dl1, Write,
			       (addr&~3), current_context, NULL, 4,
		  	       sim_cycle, NULL, NULL);
		}
	      else
		{
		  if(addr){
		    int latency = 0;
		    int pred_hit_L1 = FALSE;
		    int stack_recover_idx;
		    struct bpred_update_t dir_update;	/* bpred direction update info */
		    
		    latency = cache_access(cache_dl1, Read,
					   (addr&~3), current_context, NULL, 4,
					   sim_cycle, NULL, NULL);
		    
		    if(!mystricmp(recovery_model, "squash")){
		      /* DO LOAD LATENCY PREDICTION */
		      pred_hit_L1 =  bpred_lookup(contexts[current_context].load_lat_pred,
						  /* branch address */addr, //regs->regs_PC,
						  /* target address */ 0,
						  /* opcode */ op,
						  /* call? */ FALSE,
						  /* return? */ FALSE,
						  /* updt */&(dir_update),
						  /* RSB index */&stack_recover_idx);
		
		      bpred_update(contexts[current_context].load_lat_pred,
				   /* branch address */addr, //regs->regs_PC,
				   /* actual target address */pred_hit_L1,
				   /* taken? */latency <= cache_dl1_lat,
				   /* pred taken? */pred_hit_L1,
				   /* correct pred? */(latency <= cache_dl1_lat) == pred_hit_L1,
				   /* opcode */op,
				   /* predictor update ptr */&(dir_update));
		    }
		  }
		}
	      
	      
	      /* all loads and stores must to access D-TLB */
	      if (dtlb)
		{
		  /* access the D-DLB */
		  cache_access(dtlb, Read, (addr & ~3),
			       current_context, NULL, 4,
			       sim_cycle, NULL, NULL);
		}	      
	    }
	  
	  /* update branch predictor */
	  if (MD_OP_FLAGS(op) & F_CTRL){
	    md_addr_t pred_PC =
	      bpred_lookup(contexts[current_context].pred,
			   /* branch address */regs->regs_PC,
			   /* target address */0,
			   /* opcode */op,
			   /* call? */MD_IS_CALL(op),
			   /* return? */MD_IS_RETURN(op),
			   /* updt */&(dir_update[current_context]),
			   /* RSB index */&stack_recover_idx[current_context]);
	    
	    bpred_update(contexts[current_context].pred,
			 /* branch address */regs->regs_PC,
			 /* actual target address */regs->regs_NPC,
			 /* taken? */regs->regs_NPC != (regs->regs_PC +
							sizeof(md_inst_t)),
			 /* pred taken? */pred_PC != (regs->regs_PC + sizeof(md_inst_t)),
			 /* correct pred? */pred_PC == regs->regs_NPC,
			 /* opcode */op,
			 /* predictor update ptr */&dir_update[current_context]);
	    
	    
	  }
	  
	  /* check for DLite debugger entry condition */
	  if (dlite_check_break(regs->regs_NPC,
				is_write ? ACCESS_WRITE : ACCESS_READ,
				addr, sim_num_insn, sim_num_insn))
	    dlite_main(regs->regs_PC, regs->regs_NPC, sim_num_insn, regs, mem);
	  
	  /* go to the next instruction */
	  regs->regs_PC = regs->regs_NPC;
	  regs->regs_NPC += sizeof(md_inst_t);
	  
	  /* one more instruction has been executed */
	  contexts[current_context].fastfwd_left--;
	}
    }
  
  /* set up timing simulation entry state */
  for(i=0;i<num_contexts;i++){
    contexts[i].fetch_regs_PC = contexts[i].regs.regs_PC;
    contexts[i].fetch_pred_PC = contexts[i].regs.regs_PC;
    contexts[i].regs.regs_PC = contexts[i].regs.regs_PC;
  }
  
  /* init pipeline structs for full simulation */
  iq = calloc(iq_size, sizeof(enum iq_entry));
  
  for(i=0;i<iq_size;i++){
    *iq = IQ_ENTRY_FREE;
  }

  /* resets cache stats after fast forwarding */
  if(cache_il1){
    reset_cache_stats(cache_il1);
  }
  if(cache_dl1){
    reset_cache_stats(cache_dl1);
  }
  if(cache_il2){
    reset_cache_stats(cache_il2);
  }
  if(cache_dl2){
    reset_cache_stats(cache_dl2);
  }
  if(itlb){
    reset_cache_stats(itlb);
  }
  if(dtlb){
    reset_cache_stats(dtlb);
  }
  
  /* reset bpred stats */
  {
    int i;
    for(i=0;i<num_contexts;i++){
      bpred_after_priming(contexts[i].pred);
      bpred_after_priming(contexts[i].load_lat_pred);
    }
  }

  fprintf(stderr, "sim: ** starting performance simulation **\n");

  current_context = 0;

  /* main simulator loop, NOTE: the pipe stages are traverse in reverse order
     to eliminate this/next state synchronization and relaxation problems */
  for (;;)
    {

      int i;
      for(i=0;i<num_contexts;i++){
	/* ROB/LSQ sanity checks */
	if (contexts[i].ROB_num < contexts[i].LSQ_num)
	  panic("ROB_num < LSQ_num");
	if (((contexts[i].ROB_head + contexts[i].ROB_num) % ROB_size) != contexts[i].ROB_tail)
	  panic("ROB_head/ROB_tail wedged");
	if (((contexts[i].LSQ_head + contexts[i].LSQ_num) % LSQ_size) != contexts[i].LSQ_tail)
	  panic("LSQ_head/LSQ_tail wedged");
      }

      /* added for Wattch to clear hardware access counters */
      clear_access_stats();

      /* check if pipetracing is still active */
      ptrace_check_active(contexts[current_context].regs.regs_PC, sim_num_insn, sim_cycle);

      /* indicate new cycle in pipetrace */
      ptrace_newcycle(sim_cycle);

      /* commit entries from ROB/LSQ to architected register file */
      {
	/* commit COMMIT_WIDTH intsructions from each context each cycle */
	int i;
	for(i=0;i<num_contexts;i++)
	    commit(i);
      }
      /* service function unit release events */
      rob_release_fu();

      /* ==> may have ready queue entries carried over from previous cycles */
      /* service result completions, also readies dependent operations */
      /* ==> inserts operations into ready queue --> register deps resolved */
      writeback();

      /* try to locate memory operations that are ready to execute */
      /* ==> inserts operations into ready queue --> mem deps resolved */
      {
	//refresh each thread
	int i;
	for(i=0;i<num_contexts;i++)
	  lsq_refresh(i);
      }

      /* issue operations ready to execute from a previous cycle */
      /* <== drains ready queue <-- ready operations commence execution */
      /* scheduling occurs in two phases: instruction wakeup and instruction selection */
      /* wakeup instructions once their source opearnds are ready (speculative on loads) */
      wakeup();
     
      /* select among the ready instructions for functional units and issue them to begin RF access */
      selection();

      /* actually begin the execution of instructions on the functional units */
      execute();


      /* dispatch instructions to the IQ */
      dispatch();

      /* decode and rename new operations */
      /* ==> insert ops w/ no deps or all regs ready --> reg deps resolved */
      register_rename();

      //use the icount fetch policy
      if(!mystricmp(fetch_policy, "icount")){
	icount_fetch();
      }else if(!mystricmp(fetch_policy, "round_robbin")){    
	RR_fetch(current_context);
	current_context = (current_context + 1) % num_contexts;
      }else{
        panic("Invalid fetch policy!\n");
      }


      {
	/* decriment the fetch-issue delay counters (used for min. branch mispred. penalty) */
	int i;
	for(i=0;i<num_contexts;i++)
	  if(contexts[i].fetch_issue_delay)
	    contexts[i].fetch_issue_delay--;
      }


      /* Added by Wattch to update per-cycle power statistics */
      update_power_stats();
      
      /* go to next cycle */
      sim_cycle++;

      /* finish early? */
      /* execute until the first thread reaches max_insts */
      {
	int i;
	int done = 1;
	for(i=0;i<num_contexts;i++){
	  done = 0;
	  if(!max_insts || contexts[i].sim_num_insn >= max_insts)
	    return;
	}
	if(done)
	  return;
      }
    }
}

/*
 * Initalizes the context
 */

void init_context(context* c, int c_id){
  char str[20];

  /* allocate and initialize register file */
  regs_init(&c->regs);
  regs_init(&c->spec_regs);

  c->regs.context_id = c_id;

  c->spec_mode = FALSE;
  c->ptrace_seq = 0;


  {
    int i;
    for(i=c_id*32; i< (c_id*32 + 32); i++){
      int_reg_file[i].state = REG_ARCH;
      fp_reg_file[i].state = REG_ARCH;
      int_reg_file[i].ready = 0;
      fp_reg_file[i].ready = 0;
      int_reg_file[i].spec_ready = 0;
      fp_reg_file[i].spec_ready = 0;
      
    }

    //init rename table
    for(i=0; i<32; i++){
      //integer portion of the rename table
      c->rename_table[i] = i + c_id*32;
    }
    for(i=32; i<64; i++){
      //floating point portion of the rename table
      c->rename_table[i] = (i-32) + c_id*32;
    }

  }


  /* allocate and initialize memory space */
  sprintf(str,"mem_%d",c->id);
  c->mem = mem_create(str);
  mem_init(c->mem);
  c->mem->ld_text_base = 0;
  c->mem->ld_text_size = 0;
  c->mem->ld_data_base = 0;
  c->mem->ld_brk_point = 0;
  c->mem->ld_data_size = 0;
  c->mem->ld_stack_base = 0;
  c->mem->ld_stack_size = 0;
  c->mem->ld_stack_min = -1;
  c->mem->ld_prog_fname = NULL;
  c->mem->ld_prog_entry = 0;
  c->mem->ld_environ_base = 0;
  c->mem->ld_target_big_endian = 0;
  c->mem->context_id = c_id;
  c->id = c_id;
  c->regs.context_id = c_id;

  c->fetch_issue_delay = 0;
  c->icount = 0;
  c->sim_num_insn = 0;

}


/*
 * Prints statistics at the end of simulation.
 * This is the easiest place to add your own output.
 */
void smt_print_stats(){

  //print my STATS
  fprintf(stderr, "\n******* SMT STATS *******\n");
  fprintf(stderr, "THROUGHPUT IPC: %1.4f\n\n",sim_num_insn / (float)sim_cycle);

}
